define(['../../../js/common'], function() {

    require(['template', 'ztree', 'form', 'select2', 'MDialog', 'umeditor.config', 'umeditor'], function(template) {

        var siteId = util.get_query('siteId'),
            id = util.get_query('id');

        document.title = (id ? '编辑文章' : '添加文章') + ' - CMS内容管理系统';

        $('#txt_header').text(id ? '编辑文章' : '添加文章');

        var nav = $('#nav_tools'),
            container = $('#editor_container'),
            editor = {},
            save = function(publish, callback) {
                var title = $('#txt_title', container).val(),
                    content = editor.getContent(),
                    category = $('#txt_category', container).val(),
                    summary = $('#txt_summary', container).val();

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
                    publish: publish
                }, function(r) {

                    r = handleException(r);

                    if (!r || r.code < 0) {

                        alert(r.msg || '发生未知错误，请刷新页面后尝试');
                        return false;
                    }

                    callback && callback.apply(this, [r]);
                }, 'json');
            },
            init = function(data) {

                //构造HTML
                container.html(template('post_edit_form', data));

                //init umeditor
                editor = UM.getEditor('txt_content', {
                    initialFrameWidth: '100%',
                    initialFrameHeight: $(window).height() - 255
                });

                //select category
                $('#btn_category', container).on('click', function() {

                    var p_tree = $('<ul class="ztree"></ul>'),
                        selected = { title: '', id: '' },
                        dlg = $M({
                            title: '选择父级栏目',
                            content: p_tree[0],
                            lock: true,
                            width: '250px',
                            height: '250px',
                            ok: function() {

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
                            onClick: function(event, tId, node) {
                                selected = { title: node.name, id: node.id };
                            }
                        }
                    });

                    return false;
                });
            };

        //保存为草稿
        $('.btn_save', nav).on('click', function() {

            save(false, function(r) {

                r.code == 1 && alert('已保存为草稿，可继续编辑');
            });

            return false;
        });

        //保存并发布        
        $('.btn_publish', nav).on('click', function() {

            save(true, function(r) {

                r.code == 1 && alert('已成功保存并发布该文章');
                window.close();
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
        }).done(function(r) {

            if (!r || r.code < 0) {
                alert(r.msg || '发生未知错误，请刷新页面后尝试');

                window.close();
                return false;
            }

            init(r);
        });
    });
});