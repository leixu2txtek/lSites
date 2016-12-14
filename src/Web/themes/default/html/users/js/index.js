define(['../../../js/common'], function () {


    document.title = '用户管理 - CMS内容管理系统';

    require(['template', 'moment', 'select2', 'select2.cn', 'form', 'paging'], function (template, moment) {

        template.helper('format_date', function (date) {

            return moment(date).format('YYYY-MM-DD');
        });

        var nav = $('#nav_tools'),
            siteId = util.get_query('siteId'),
            form = $('#users_form');

        $('#btn_add_user', nav).on('click', function () {

            var edit_form = $(template('users_edit_form', { siteId: util.get_query('siteId') })),
                dlg = $M({
                    title: '添加用户信息',
                    content: edit_form[0],
                    lock: true,
                    width: '400px',
                    height: '260px',
                    position: '50% 50%',
                    ok: function () {

                        edit_form.submit();
                    },
                    okVal: '保存',
                    cancel: false,
                    cancelVal: '取消'
                });

            $('[name=permission]', edit_form).select2({ minimumResultsForSearch: -1 });

            edit_form.gform({
                url: config.host + 'user/save',
                onSuccess: function (r) {

                    r = handleException(r);

                    if (!r) return false;
                    if (r.code < 0) {

                        alert(r.msg || '发生未知错误，请刷新后尝试');
                        return false;
                    }

                    dlg.close();
                    form.submit();
                }
            });

            return false;
        });

        $('#btn_add_exist_user', nav).on('click', function () {

            var users_form = $(template('exist_user_form', { site_id: siteId })),
                dlg = $M({
                    title: '添加已有用户',
                    content: users_form[0],
                    lock: true,
                    width: '450px',
                    height: '120px',
                    position: '50% 50%',
                    ok: function () {

                        users_form.submit();
                    },
                    okVal: '保存',
                    cancel: false,
                    cancelVal: '取消'
                });

            $('[name=userId]', users_form).select2({
                placeholder: '请选择用户',
                language: 'zh-CN',
                ajax: {
                    url: config.host + 'user/exists',
                    dataType: 'json',
                    delay: 250,
                    data: function (params) {
                        return { userName: params.term, page: params.page, siteId: siteId };
                    },
                    processResults: function (data, params) {

                        params.page = params.page || 1;

                        return {
                            results: data.results,
                            pagination: { more: (params.page * 10) < data.total_count }
                        };
                    },
                    cache: true
                },
                templateResult: function (data) { return data.text; },
                templateSelection: function (data) { return data.text; }
            });

            $('[name=permission]', users_form).select2({ minimumResultsForSearch: -1 });

            users_form.gform({
                url: config.host + 'user/add_exist_user',
                beforeSubmit: function () {

                    var user = $('[name=userId]', users_form).select2('val'),
                        permission = $('[name=permission]', users_form).select2('val');

                    if (!user || user.length == 0) {

                        alert('请选择要添加的用户');

                        $('[name=userId]', users_form).select2('open');
                        return false;
                    }

                    if (!permission || permission.length == 0) {

                        alert('请选择用户的权限');

                        $('[name=permission]', users_form).select2('open');
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
                    form.submit();
                }
            });

            return false;
        });

        if (form.find('input[name=siteId]').length == 0) {
            form.append('<input type="hidden" name="siteId" value="' + siteId + '" />');
        }

        $('select', form).select2({ minimumResultsForSearch: -1, allowClear: true });

        form.gform({
            url: config.host + 'user/list',
            onSuccess: function (r) {
                r = handleException(r);

                if (!r) return false;
                if (r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新后尝试');
                    return false;
                }

                r.site_id = siteId;

                //更新总数
                $('#total_count', nav).html(r.paging.total_count);
                $('#table_container', form).html(template('users_table', r));

                //绑定表格                
                var table = $('table', form).gtable();

                //删除用户
                $('.delete', table).on('click', function () {

                    if (!confirm('是否确认删除该用户')) return false;

                    $.post(config.host + 'user/delete', {
                        siteId: siteId,
                        userId: $(this).data('id')
                    }, function (r) {

                        r = handleException(r);

                        if (!r) return false;
                        if (r.code < 0) {

                            alert(r.msg || '发生未知错误，请刷新后尝试');
                            return false;
                        }

                        alert('已彻底删除该用户');
                        form.submit();

                    }, 'json');

                    return false;
                });

                //编辑用户信息                
                $('.edit', table).on('click', function () {

                    $.post(config.host + 'user/detail', {
                        siteId: siteId,
                        userId: $(this).data('id')
                    }, function (r) {

                        r = handleException(r);

                        if (!r) return false;
                        if (r.code < 0) {

                            alert(r.msg || '发生未知错误，请刷新后尝试');
                            return false;
                        }

                        r.data.siteId = siteId;

                        var edit_form = $(template('users_edit_form', r.data)),
                            dlg = $M({
                                title: '修改用户信息',
                                content: edit_form[0],
                                lock: true,
                                width: '400px',
                                height: '260px',
                                position: '50% 50%',
                                ok: function () {

                                    edit_form.submit();
                                },
                                okVal: '保存',
                                cancel: false,
                                cancelVal: '取消'
                            });

                        $('[name=permission]', edit_form).select2({
                            minimumResultsForSearch: -1
                        }).val($('[name=permission]', edit_form).data('selected')).trigger('change');

                        edit_form.gform({
                            url: config.host + 'user/save',
                            onSuccess: function (r) {

                                r = handleException(r);

                                if (!r) return false;
                                if (r.code < 0) {

                                    alert(r.msg || '发生未知错误，请刷新后尝试');
                                    return false;
                                }

                                dlg.close();
                                form.submit();
                            }
                        });

                    }, 'json');

                    return false;
                });

                //重置密码
                $('.reset', table).on('click', function () {

                    if (!confirm('是否确认重置密码？')) return false;

                    $.post(config.host + 'user/reset', { userId: $(this).data('id'), siteId: siteId }, function (r) {

                        r = handleException(r);

                        if (!r) return false;
                        if (r.code < 0) {

                            alert(r.msg || '发生未知错误，请刷新后尝试');
                            return false;
                        }

                        form.submit();

                    }, 'json');

                    return false;
                });

                //绑定分页信息                
                $('.x-paging-container', form).paging(r.paging);
            },
            callback: function (form) { form.submit(); }
        });
    });
});