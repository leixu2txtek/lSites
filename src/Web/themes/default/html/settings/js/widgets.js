define(['../../../js/common'], function () {

    document.title = '挂件列表 - CMS内容管理系统';

    require(['template', 'moment', 'select2', 'form', 'paging'], function (template, moment) {

        template.helper('format_date', function (date) {
            return moment(date).format('YYYY-MM-DD');
        });

        var form = $('#post_form'),
            nav = $('#nav_tools'),
            siteId = util.get_query('siteId');

        $('select', form).select2({
            minimumResultsForSearch: -1,
            allowClear: true
        });

        if (form.find('input[name=siteId]').length == 0) {
            form.append('<input type="hidden" name="siteId" value="' + siteId + '" />');
        }

        //添加新挂件
        $('#btn_add', nav).on('click', function () {

            var add_form = $(template('widget_add_form', {
                site_id: siteId
            })),
                dlg = $M({
                    title: '添加新挂件',
                    content: add_form[0],
                    width: '450px',
                    lock: true,
                    position: '50 % 50 %',
                    ok: function () {
                        add_form.submit();
                    },
                    okVal: '保存',
                    cancel: false,
                    cancelVal: '取消'
                });

            add_form.gform({
                url: config.host + 'widget/save',
                beforeSubmit: function () {
                    var name = $('[name=name]', add_form).val();

                    if (name.length == 0) {

                        $('[name=name]', add_form).parent().addClass('has-error');
                        alert('名称不能为空');

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

                    alert('添加成功');
                    form.submit(); //重新刷新挂件列表
                }
            });

        });

        //导出挂件
        $('#btn_export', nav).on('click', function () {

            window.location.href = '/widget/export?siteId=' + siteId;
            return false;
        });

        //导入挂件
        $('#btn_import', nav).on('click', function () {

            var import_form = $(template.compile('<form class="add-form" method="post"><input type="hidden" name="siteId" value="{{site_id}}" /><div class="addForm-input"><label class="addForm-label"><em class="iconfont"></em>数据文件：</label><input style="padding: 2px 4px; height: 34px; line-height: 30px;" type="file" name="file" class="form-control"></div></form>')({ site_id: siteId })),
                dlg = $M({
                    title: '导入挂件',
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
                url: config.host + 'widget/import',
                beforeSubmit: function () {

                    var file = $('[name=file]', import_form).val();

                    if (file.length == 0) {

                        $('[name=file]', import_form).parent().addClass('has-error');

                        alert('要导入的挂件数据不能为空');
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
                    form.submit(); //重新刷新挂件列表
                }
            });

            return false;
        });

        // 绑定表单
        form.gform({
            url: config.host + 'widget/list',
            onSuccess: function (r) {

                r = handleException(r);

                if (!r) return false;
                if (r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新后尝试');
                    return false;
                }

                //更新总数
                $('#total_count').html(r.paging.total_count);
                $('#table_container', form).html(template('widgets_table', r));

                //绑定表格                
                var table = $('table', form).gtable();

                // 编辑站点
                $('.edit', table).on('click', function () {
                    var siteId = util.get_query('siteId'),
                        id = $(this).data('id');

                    $.post(config.host + 'widget/detail', {
                        id: $(this).data('id'),
                        siteId: siteId
                    }, function (r) {

                        r = handleException(r);

                        if (!r) return false;
                        if (r.code < 0) {

                            alert(r.msg || '发生未知错误，请刷新后尝试');
                            return false;
                        }

                        var edit_form = $(template('widget_add_form', r.widget)),
                            dlg = $M({
                                title: '编辑挂件',
                                content: edit_form[0],
                                width: '450px',
                                lock: true,
                                position: '50% 50%',
                                ok: function () {
                                    edit_form.submit();
                                },
                                okVal: '保存',
                                cancel: false,
                                cancelVal: '取消'
                            });

                        edit_form.gform({
                            url: config.host + 'widget/save',
                            beforeSubmit: function () {

                                var name = $('[name=name]', edit_form).val();

                                if (name.length == 0) {

                                    $('[name=name]', edit_form).parent().addClass('has-error');
                                    alert('名称不能为空');

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

                                alert('修改成功');
                                form.submit(); //重新刷新挂件列表
                            }
                        });

                    }, 'json');
                });

                // 删除挂件
                $('.delete', table).on('click', function () {

                    var siteId = util.get_query('siteId'),
                        id = $(this).data('id');

                    if (!confirm('是否确定彻底删除此挂件')) return false;

                    var delete_site = function (confirmed) {

                        $.post(config.host + 'widget/delete', {
                            id: id,
                            siteId: siteId,
                            confirmed: confirmed || false
                        }, function (r) {

                            r = handleException(r);

                            if (!r) return false;
                            if (r.code < 0) {

                                alert(r.msg || '发生未知错误，请刷新后尝试');
                                return false;
                            }

                            alert('已彻底删除该挂件');
                            form.submit();

                        }, 'json');
                    };

                    delete_site(false);
                });

                //绑定分页信息                
                $('.x-paging-container', form).paging(r.paging);
            },
            callback: function (form) { form.submit(); }
        });
    });
});