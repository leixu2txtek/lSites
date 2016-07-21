define(['../../../js/common'], function () {

    document.title = '站点列表 - 站群管理';

    require(['template', 'moment', 'select2', 'form', 'paging', 'MDialog'], function (template, moment) {

        template.helper('format_date', function (date) {

            return moment(date).format('YYYY-MM-DD');
        });

        var form = $('#site_form'),
            nav = $('#nav_tools');

        $('select', form).select2({
            minimumResultsForSearch: -1,
            allowClear: true
        });

        // 添加新站点    
        $('.add-site', nav).on('click', function () {

            var add_form = $(template('site_add_form', {})),
                dlg = $M({
                    title: '添加新站点',
                    content: add_form[0],
                    width: '450px',
                    height: '350px',
                    position: '50% 50%',
                    ok: function () {

                        if (!confirm('是否确认添加站点？')) return false;

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
                        domain = $('[name=domain]', add_form).val();

                    if (title.length == 0) {

                        alert('站点名称不能为空');
                        return false;
                    }

                    return false;
                },
                onSuccess: function (r) {

                    if (!r || r.code < 0) {

                        alert(r.msg || '发生未知错误，请刷新后尝试');
                        return false;
                    }

                    dlg.close();

                    alert('已成功添加站点信息');
                    form.submit();      //重新刷新站点列表
                }
            });
        });

        //绑定表单
        form.gform({
            url: config.host + 'site/list',
            onSuccess: function (r) {

                if (!r || r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新后尝试');
                    return false;
                }

                //更新总数
                $('#total_count').html(r.paging.total_count);
                $('#table_container', form).html(template('site_table', r));

                //绑定表格                
                var table = $('table', form).gtable();

                //TODO 绑定按钮事件

                $('.edit', table).on('click', function () {

                });

                $('.delete', table).on('click', function () {

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