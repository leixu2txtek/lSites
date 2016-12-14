define(['../../../js/common'], function () {

    document.title = '站点列表 - 站群管理';

    require(['template', 'moment', 'select2', 'form', 'paging'], function (template, moment) {

        template.helper('format_date', function (date) {

            return moment(date).format('YYYY-MM-DD');
        });

        var form = $('#site_form'),
            nav = $('#nav_tools');

        $('select', form).select2({ minimumResultsForSearch: -1, allowClear: true });

        // 添加新站点    
        $('.add-site', nav).on('click', function () {

            var add_form = $(template('site_add_form', {})),
                dlg = $M({
                    title: '添加新站点',
                    content: add_form[0],
                    width: '450px',
                    lock: true,
                    position: '50% 50%',
                    ok: function () {

                        add_form.submit();
                    },
                    okVal: '保存',
                    cancel: false,
                    cancelVal: '取消'
                });

            add_form.gform({
                url: config.host + 'site/save',
                beforeSubmit: function () {

                    var title = $('[name=title]', add_form).val(),
                        domain = $('[name=domain]', add_form).val(),
                        keyWords = $('[name=keyWords]', add_form).val();

                    if (title.length == 0) {

                        $('[name=title]', add_form).parent().addClass('has-error');
                        alert('名称不能为空');

                        return false;
                    }

                    if (domain.length == 0) {

                        $('[name=domain]', add_form).parent().addClass('has-error');
                        alert('域名不能为空');

                        return false;
                    }

                    if (keyWords.length == 0) {

                        $('[name=keyWords]', add_form).parent().addClass('has-error');
                        alert('关键字不能为空');

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

                    alert('已成功添加站点信息');
                    form.submit(); //重新刷新站点列表
                }
            });
        });

        //绑定表单
        form.gform({
            url: config.host + 'site/list',
            onSuccess: function (r) {

                r = handleException(r);

                if (!r) return false;
                if (r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新后尝试');
                    return false;
                }

                //更新总数
                $('#total_count').html(r.paging.total_count);
                $('#table_container', form).html(template('site_table', r));

                //绑定表格                
                var table = $('table', form).gtable();

                $('.edit', table).on('click', function () {

                    $.post(config.host + 'site/detail', {
                        id: $(this).data('id')
                    }, function (r) {

                        r = handleException(r);

                        if (!r) return false;
                        if (r.code < 0) {

                            alert(r.msg || '发生未知错误，请刷新后尝试');
                            return false;
                        }

                        var edit_form = $(template('site_add_form', r.data)),
                            dlg = $M({
                                title: '编辑站点',
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
                            url: config.host + 'site/save',
                            beforeSubmit: function () {

                                var title = $('[name=title]', edit_form).val(),
                                    domain = $('[name=domain]', edit_form).val(),
                                    keyWords = $('[name=keyWords]', edit_form).val();

                                if (title.length == 0) {

                                    $('[name=title]', edit_form).parent().addClass('has-error');
                                    alert('名称不能为空');

                                    return false;
                                }

                                if (domain.length == 0) {

                                    $('[name=domain]', edit_form).parent().addClass('has-error');
                                    alert('域名不能为空');

                                    return false;
                                }

                                if (keyWords.length == 0) {

                                    $('[name=keyWords]', edit_form).parent().addClass('has-error');
                                    alert('关键字不能为空');

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

                                dlg.close(); //关闭dlg

                                alert('已修改添加站点信息');
                                form.submit(); //重新刷新站点列表
                            }
                        });

                    }, 'json');
                });

                $('.delete', table).on('click', function () {

                    var siteId = util.get_query('siteId'),
                        id = $(this).data('id');

                    if (!confirm('是否确定彻底删除此站点')) return false;

                    var delete_site = function (confirmed) {

                        $.post(config.host + 'site/delete', {
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

                            if (r.code == 2 && !confirm('指定的站点下有挂件内容，若确认删除则挂件内容也一并删除，是否确认删除')) return false;

                            if (r.code == 2) {

                                delete_site(true);
                                return false;
                            }

                            alert('已彻底删除该站点');
                            form.submit();

                        }, 'json');
                    };

                    delete_site(false);
                });

                //绑定分页信息                
                $('.x-paging-container', form).paging(r.paging);
            },
            callback: function (form) {
                form.submit();
            }
        });
    });
});