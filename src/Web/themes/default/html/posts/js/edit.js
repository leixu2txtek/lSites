define(['../../../js/common'], function () {

    require(['template', 'ztree', 'form', 'select2', 'ue', 'ue.file', 'ue.video'], function (template) {

        var siteId = util.get_query('siteId'),
            id = util.get_query('id');

        document.title = (id ? '编辑文章' : '添加文章') + ' - CMS内容管理系统';

        $('#txt_header').text(id ? '编辑文章' : '添加文章');

        var nav = $('#nav_tools'),
            container = $('#editor_container'),
            editor = {},
            save = function (e, publish, callback) {

                var title = $('#txt_title', container).val(),
                    content = editor.getContent(),
                    category = $('#txt_category', container).val(),
                    summary = $('#txt_summary', container).val(),
                    sort_order = $('#txt_sort_order', container).val(),
                    view_count = $('#txt_view_count', container).val(),
                    date_created = $('#txt_date_created', container).val(),
                    is_top = $('[name=cb_top]', container).val(),
                    props = {};

                if (title.length == 0) {

                    alert('文章的标题不能为空');
                    $('#txt_title', container).focus();

                    e.data('pending', false);
                    return false;
                }

                if (content.length == 0) {

                    alert('文章的内容不能为空');
                    editor.focus(true);

                    e.data('pending', false);
                    return false;
                }

                if (category.length == 0) {

                    alert('文章的栏目不能为空');
                    $('#btn_category', container).trigger('click');

                    e.data('pending', false);
                    return false;
                }

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

                if (has_error) {

                    e.data('pending', false);
                    return false;
                }

                $.post(config.host + 'posts/save', {
                    siteId: siteId,
                    id: id,
                    title: title,
                    subTitle: '',
                    content: content,
                    summary: summary,
                    categoryId: category,
                    viewCount: view_count,
                    sortOrder: sort_order,
                    publish: publish,
                    dateCreated: date_created,
                    isTop: is_top,
                    props: JSON.stringify(props)
                }, function (r) {

                    r = handleException(r);

                    if (!r) return false;
                    if (r.code < 0) {

                        alert(r.msg || '发生未知错误，请刷新后尝试');

                        e.data('pending', false);
                        return false;
                    }

                    callback && callback.apply(this, [r]);
                }, 'json');
            },
            init = function (data) {

                var props = JSON.parse(data.post.props || '{}');

                data.post.props = [];

                for (var p in props) {

                    data.post.props.push({
                        key: p,
                        value: props[p]
                    });
                }

                //构造HTML
                container.html(template('post_edit_form', data));

                //获取图片，文件，视频上传配置
                $.ajax({
                    url: config.host + 'config/get',
                    type: 'POST',
                    dataType: 'json',
                    data: {
                        siteId: siteId
                    },
                    async: false
                }).done(function (r) {

                    if (!r || r.code < 0) {
                        alert(r.msg || '发生未知错误，请刷新页面后尝试');

                        window.close();
                        return false;
                    }

                    //init ueditor
                    editor = UE.getEditor('txt_content', {
                        initialFrameWidth: '100%',
                        initialFrameHeight: $(window).height() - 270,
                        image: r.image,
                        file: r.file,
                        video: r.video
                    });
                });

                //select category
                $('#btn_category', container).on('click', function () {

                    var p_tree = $('<ul class="ztree" style="max-height:275px;max-width:280px;overflow:auto;"></ul>'),
                        selected = {
                            title: '',
                            id: ''
                        },
                        dlg = $M({
                            title: '选择栏目信息',
                            content: p_tree[0],
                            lock: true,
                            width: '300px',
                            height: '300px',
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
                            url: config.host + 'category/list_with_permission',
                            autoParam: ['id=parentId'],
                            otherParam: {
                                'siteId': util.get_query('siteId')
                            },
                            type: "post"
                        },
                        callback: {
                            onClick: function (event, tId, node) {
                                selected = {
                                    title: node.name,
                                    id: node.id
                                };
                            }
                        }
                    });

                    return false;
                });

                //select is_top
                $('[name=cb_top]', container).select2({
                    minimumResultsForSearch: -1
                }).val($('[name=cb_top]', container).data('selected').toString()).trigger('change');
            };

        //保存为草稿
        $('.btn_save', nav).on('click', function () {

            var _this = $(this);

            if (_this.data('disable') || _this.data('pending')) return false;

            _this.data('pending', true);

            save(_this, false, function (r) {

                _this.data('pending', false);

                if (r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新页面后尝试');
                    return false;
                }

                //保存草稿后将本文章的ID更新为保存后的ID，防止保存&发布时出现多个相同的文章
                id = r.id;

                alert('已保存为草稿，可继续编辑');
            });
        });

        //保存并发布        
        $('.btn_publish', nav).on('click', function () {

            var _this = $(this);

            if (_this.data('pending')) return false;

            _this.data('pending', true);

            save(_this, true, function (r) {

                _this.data('pending', false);

                alert(r.is_pending ? '已保存文章，并提交至审核，待审核通过后可展示' : '已成功保存并发布文章');

                window.history.go(-1);
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
            data: {
                siteId: siteId,
                id: id
            },
            async: false
        }).done(function (r) {

            if (!r || r.code < 0) {
                alert(r.msg || '发生未知错误，请刷新页面后尝试');

                window.close();
                return false;
            }

            init(r);

            if (r.post.is_published) {

                //更新时不需要保存草稿
                $('.btn_save').data('disable', true);

                $('.btn_publish').text('更新').prop('title', '更新当前文章内容');
            }
        });
    });
});