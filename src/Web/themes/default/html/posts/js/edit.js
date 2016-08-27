define(['../../../js/common'], function () {

    require(['template', 'ztree', 'form', 'select2', 'MDialog', 'ueditor.config', 'ueditor'], function (template) {

        var siteId = util.get_query('siteId'),
            id = util.get_query('id');

        document.title = (id ? '编辑文章' : '添加文章') + ' - CMS内容管理系统';

        $('#txt_header').text(id ? '编辑文章' : '添加文章');

        var nav = $('#nav_tools'),
            container = $('#editor_container'),
            editor = {},
            save = function (publish, callback) {
                var title = $('#txt_title', container).val(),
                    content = editor.getContent(),
                    category = $('#txt_category', container).val(),
                    summary = $('#txt_summary', container).val(),
                    props = {};

                //处理自定义属性
                var has_error = false;
                $('.prop_key', container).each(function (i, v) {

                    var key = $(this).val();

                    if (key.length == 0) return;

                    if (props[key]) {

                        has_error = true;
                        alert('自定义属性的键不能重复');
                        return false;
                    }

                    props[key] = $(this).next('.prop_value').val();
                });

                if (has_error) return;

                $.post(config.host + 'posts/save', {
                    siteId: siteId,
                    id: id,
                    title: title,
                    subTitle: '',
                    content: content,
                    summary: summary,
                    categoryId: category,
                    viewCount: 0,
                    sortOrder: 0,
                    publish: publish,
                    props: JSON.stringify(props)
                }, function (r) {

                    r = handleException(r);

                    if (!r || r.code < 0) {

                        alert(r.msg || '发生未知错误，请刷新页面后尝试');
                        return false;
                    }

                    callback && callback.apply(this, [r]);
                }, 'json');
            },
            init = function (data) {

                var props = JSON.parse(data.post.props || '{}');

                data.post.props = [];

                for (var p in props) {

                    data.post.props.push({ key: p, value: props[p] });
                }

                //构造HTML
                container.html(template('post_edit_form', data));

                //init ueditor
                editor = UE.getEditor('txt_content', {
                    initialFrameWidth: '100%',
                    initialFrameHeight: $(window).height() - 255,
                    imageActionName: '/image/upload?siteId=' + siteId,
                    imageAllowFiles: [".jpeg", ".jpg", ".png", ".gif"],
                    imageMaxSize: 1024 * 1024 * 5,
                    imageCompressBorder: 1600,
                    imageCompressEnable: true,
                    imageInsertAlign: "none",
                    imageUrlPrefix: ""
                });

                //select category
                $('#btn_category', container).on('click', function () {

                    var p_tree = $('<ul class="ztree"></ul>'),
                        selected = { title: '', id: '' },
                        dlg = $M({
                            title: '选择栏目信息',
                            content: p_tree[0],
                            lock: true,
                            width: '250px',
                            height: '250px',
                            ok: function () {

                                dlg.close();

                                $('#txt_category', container).val(selected.id);
                                $('#btn_category', container).text(selected.title || '选择栏目');
                            },
                            okVal: '保存',
                            cancel: false,
                            cancelVal: '取消'
                        });

                    p_tree = $.fn.zTree.init(p_tree, {
                        async: {
                            enable: true,
                            url: config.host + 'category/list',
                            autoParam: ['id=parentId'],
                            otherParam: { 'siteId': util.get_query('siteId') },
                            type: "post"
                        },
                        callback: {
                            onClick: function (event, tId, node) {
                                selected = { title: node.name, id: node.id };
                            }
                        }
                    });

                    return false;
                });
            };

        //保存为草稿
        $('.btn_save', nav).on('click', function () {

            save(false, function (r) {

                r.code == 1 && alert('已保存为草稿，可继续编辑');
            });
        });

        //保存并发布        
        $('.btn_publish', nav).on('click', function () {

            save(true, function (r) {

                r.code == 1 && alert('已成功保存并发布该文章');
            });

            return false;
        });

        //添加
        if (!id) {

            init({ post: { category: {} } });

            return false;
        }

        $.ajax({
            url: config.host + 'posts/detail',
            type: 'POST',
            dataType: 'json',
            data: { siteId: siteId, id: id },
            async: false
        }).done(function (r) {

            if (!r || r.code < 0) {
                alert(r.msg || '发生未知错误，请刷新页面后尝试');

                window.close();
                return false;
            }

            init(r);
        });
    });
});