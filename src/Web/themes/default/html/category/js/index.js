define(['../../../js/common'], function () {

    document.title = '栏目管理 - CMS内容管理系统';

    require(['template', 'ztree', 'form', 'select2'], function (template) {

        var site_id = util.get_query('siteId');

        //init ztree
        var tree = $.fn.zTree.init($("#category_tree"), {
            async: {
                enable: true,
                url: config.host + 'category/list',
                autoParam: ['id=parentId'],
                otherParam: {
                    'siteId': site_id
                },
                type: "post"
            },
            callback: {
                onClick: function (event, tId, node) {

                    //绑定点击ztree后事件
                    $.post(config.host + 'category/detail', {
                        id: node.id,
                        siteId: util.get_query('siteId')
                    }, function (r) {

                        if (!r || r.code < 0) {

                            alert(r.msg || '发生未知错误，请刷新尝试');

                            return false;
                        }

                        var form = $(template('category_form', {
                            item: r.data
                        }));

                        $('.btn_delete', form).on('click', function () {

                            var id = $(this).data('id');

                            if (!confirm('确认删除该栏目？')) return false;

                            $.post(config.host + 'category/delete', {
                                id: node.id,
                                siteId: util.get_query('siteId')
                            }, function (r) {

                                if (!r || r.code < 0) {

                                    alert(r.msg || '发生未知错误，请刷新尝试');

                                    return false;
                                }

                                alert('删除成功');

                                //刷新变更后数据
                                tree.reAsyncChildNodes(node.getParentNode(), "refresh");
                            }, 'json');
                        });

                        $('select', form).select2({
                            minimumResultsForSearch: -1
                        });

                        //选择父级栏目(右侧选择栏目)
                        $('#select_parent', form).on('click', function () {

                            select_parent(function (node) {

                                $('[name=parentId]', form).val(node.id);
                                $('#parent_name', form).val(node.title);

                                $('#column-close', form).show();
                            });
                        });

                        $('#column-close', form).on('click', function () {

                            $('[name=parentId]', form).val('');
                            $('#parent_name', form).val('');

                            $(this).hide();

                            return false;
                        }).css('display', $('[name=parentId]', form).val() ? 'block' : 'none');

                        //选择父级栏目
                        form.gform({
                            url: config.host + 'category/save',
                            onSuccess: function (r) {

                                if (!r || r.code < 0) {
                                    alert(r.msg || '发生未知错误，请刷新尝试');
                                    return false;
                                }

                                alert('保存成功');

                                //重新加载编辑表单
                                $('.add_category', nav).trigger('click');

                                //刷新变更后数据
                                tree.reAsyncChildNodes(node.getParentNode(), "refresh");
                            }
                        });

                        $('#edit_container').html(form);

                    }, 'json');
                }
            }
        });

        //绑定添加栏目        
        var nav = $('#nav_tools');

        //添加栏目
        $('.add_category', nav).on('click', function () {

            var form = $(template('category_form', {
                item: {
                    site_id: util.get_query('siteId'),
                    parent: {}
                }
            }));

            $('select', form).select2({
                minimumResultsForSearch: -1
            });

            //选择父级栏目(右侧选择栏目)
            $('#select_parent', form).on('click', function () {

                select_parent(function (node) {

                    $('[name=parentId]', form).val(node.id);
                    $('#parent_name', form).val(node.title);

                    $('#column-close', form).show();
                });
            });

            $('#column-close', form).on('click', function () {

                $('[name=parentId]', form).val('');
                $('#parent_name', form).val('');

                $(this).hide();

                return false;
            }).css('display', $('[name=parentId]', form).val() ? 'block' : 'none');

            form.gform({
                url: config.host + 'category/save',
                onSuccess: function (r) {

                    if (!r || r.code < 0) {
                        alert(r.msg || '发生未知错误，请刷新尝试');
                        return false;
                    }

                    alert('保存成功');

                    //重新加载编辑表单
                    $('.add_category', nav).trigger('click');

                    //刷新变更后数据
                    tree.reAsyncChildNodes('', "refresh");
                }
            });

            $('#edit_container').html(form);
        }).trigger('click');

        //导出栏目
        $('#btn_export', nav).on('click', function () {

            window.location.href = '/category/export?siteId=' + site_id;
            return false;
        });

        //导入栏目
        $('#btn_import', nav).on('click', function () {

            var import_form = $(template.compile('<form class="add-form" method="post"><input type="hidden" name="siteId" value="{{site_id}}" /><div class="addForm-input"><label class="addForm-label"><em class="iconfont"></em>数据文件：</label><input style="padding: 2px 4px; height: 34px; line-height: 30px;" type="file" name="file" class="form-control"></div></form>')({ site_id: site_id })),
                dlg = $M({
                    title: '导入栏目',
                    content: import_form[0],
                    width: '450px',
                    lock: true,
                    position: '50% 50%',
                    ok: function () {

                        import_form.submit();
                    },
                    okVal: '立即导入',
                    cancel: false,
                    cancelVal: '取消'
                });

            import_form.gform({
                url: config.host + 'category/import',
                beforeSubmit: function () {

                    var file = $('[name=file]', import_form).val();

                    if (file.length == 0) {

                        $('[name=file]', import_form).parent().addClass('has-error');

                        alert('要导入的栏目数据不能为空');
                        return false;
                    }
                },
                onSuccess: function (r) {

                    r = handleException(r);

                    if (!r) return false;
                    if (r.code < 0) {

                        alert(r.msg || '发生未知错误，请刷新后尝试');
                        return false;
                    }

                    dlg.close();
                    alert('导入成功');

                    //刷新变更后数据
                    tree.reAsyncChildNodes('', "refresh");
                }
            });

            return false;
        });

        //选择父级栏目
        var select_parent = function (callback) {

            var p_tree = $('<ul class="ztree" style="max-height:275px;max-width:280px;overflow:auto;"></ul>'),
                selected = {},
                dlg = $M({
                    title: '选择父级栏目',
                    content: p_tree[0],
                    lock: true,
                    width: '300px',
                    height: '300px',
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
        };
    });
});