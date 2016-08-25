define(['../../../js/common'], function () {

    document.title = '栏目管理 - CMS内容管理系统';

    require(['template', 'ztree', 'form', 'select2', 'MDialog'], function (template) {

        //init ztree
        var tree = $.fn.zTree.init($("#category_tree"), {
            async: {
                enable: true,
                url: config.host + 'category/list',
                autoParam: ['id=parentId'],
                otherParam: { 'siteId': util.get_query('siteId') },
                type: "post"
            },
            callback: {
                onClick: function (event, tId, node) {
                    //绑定点击ztree后事件
                    $.post(config.host + 'category/detail', { id: node.id, siteId: util.get_query('siteId') }, function (r) {

                        if (!r || r.code < 0) {

                            alert(r.msg || '发生未知错误，请刷新尝试');

                            return false;

                        }

                        var form = $(template('category_form', { item: r.data }));

                        $('.btn_delete', form).on('click', function () {

                            var id = $(this).data('id');

                            if (!confirm('确认删除该栏目？')) return false;

                            $.post(config.host + 'category/delete', { id: node.id, siteId: util.get_query('siteId') }, function (r) {

                                if (!r || r.code < 0) {

                                    alert(r.msg || '发生未知错误，请刷新尝试');

                                    return false;
                                }

                                alert('删除成功');

                                //刷新变更后数据
                                tree.reAsyncChildNodes(node.getParentNode(), "refresh");
                            }, 'json');
                        });

                        $('select', form).select2({ minimumResultsForSearch: -1 });

                        //选择父级栏目
                        form.gform({
                            url: config.host + 'category/save',
                            onSuccess: function (r) {

                                if (!r || r.code < 0) {
                                    alert(r.msg || '发生未知错误，请刷新尝试');
                                    return false;
                                }

                                alert('保存成功');

                                //刷新变更后数据
                                tree.reAsyncChildNodes(node.getParentNode(), "refresh");
                            }
                        });

                        $('#edit_container').html(form);

                        columClear(node.getParentNode());

                    }, 'json');
                }
            }
        });

        // 清除栏目
        var columClear = function (node) {

            (node.id) ? $("#column-close").show() : $("#column-close").hide();

            $("#column-close").off('click').on('click', function () {

                $('#parent_name').val('');

                $("#column-close").hide();
            })
        };

        //绑定添加栏目        
        var nav = $('#nav_tools');

        $('.add_category', nav).on('click', function () {

            var form = $(template('category_form', { item: { site_id: util.get_query('siteId'), parent: {} } }));

            $('select', form).select2({
                minimumResultsForSearch: -1
            });

            //选择父级栏目(右侧选择栏目)
            $('#select_parent', form).on('click', function () {

                select_parent(function (node) {
                    $('[name=parentId]', form).val(node.id);
                    $('#parent_name', form).val(node.title);

                    //TODO 清除父级
                    columClear(node);
                });
            });

            form.gform({
                url: config.host + 'category/save',
                onSuccess: function (r) {

                    if (!r || r.code < 0) {
                        alert(r.msg || '发生未知错误，请刷新尝试');
                        return false;
                    }

                    alert('保存成功');

                    //刷新变更后数据
                    tree.reAsyncChildNodes('', "refresh");
                }
            });

            $('#edit_container').html(form);
        }).trigger('click');

        //选择父级栏目
        var select_parent = function (callback) {

            var p_tree = $('<ul class="ztree"></ul>'),
                selected = {},
                dlg = $M({
                    title: '选择父级栏目',
                    content: p_tree[0],
                    lock: true,
                    width: '250px',
                    height: '250px',
                    position: '50% 50%',
                    ok: function () {

                        dlg.close();
                        callback && callback.apply(null, [selected]);
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
        };
    });
});