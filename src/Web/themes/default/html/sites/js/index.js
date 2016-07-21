define(['../../../js/common'], function() {

    document.title = '站点列表 - 站群管理';

    require(['template', 'moment', 'select2', 'form', 'paging', 'MDialog'], function(template, moment) {

        template.helper('format_date', function(date) {

            return moment(date).format('YYYY-MM-DD');

        });

        var form = $('#site_form'),
            nav = $('#nav_tools');

        $('select', form).select2({
            minimumResultsForSearch: -1,
            allowClear: true
        });

        if (form.find('input[name=siteId]').length == 0) {
            form.append('<input type="hidden" name="siteId" value="' + util.get_query('siteId') + '" />');
        };


        // 添加新站点    
        $('.add-site', nav).on('click', function() {

            var elems = $('#addSite').html();

            dlg = $M({
                title: '添加新站点',
                content: elems,
                width: '450px',
                height: '350px',
                position: '50% 50%',
                ok: function() {
                    this.title('正在提交...');
                },
                okVal: '保存',
                cancel: false,
                cancelVal: '取消'
            });

            var siteId = util.get_query('siteId'),
                id = $(this).data('id'),
                dlg = undefined,
                site = function() {
                    $.post(config.host + 'site/save'), {
                            ids: [id],
                            siteId: siteId,
                        },
                        function(r) {

                            if (!r || r.code < 0) {
                                alert(r.msg || '发生未知错误，请刷新尝试');
                                return false;
                            }

                        }
                };
        });





        //绑定表单
        form.gform({
            url: config.host + 'site/list',
            onSuccess: function(r) {

                if (!r || r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新后尝试');
                    return false;
                }

                //更新总数
                $('#total_count').html(r.paging.total_count);
                $('#table_container', form).html(template('site_table', r));

                //绑定表格                
                var table = $('table', form).gtable();




                //绑定分页信息                
                $('.x-paging-container', form).paging(r.paging);
            },
            callback: function(form) {
                form.submit();
            },

        });
    });
});