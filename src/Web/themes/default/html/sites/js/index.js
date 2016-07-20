define(['../../../js/common'], function() {

    document.title = '站点列表 - 站群管理';

    require(['template', 'moment', 'select2', 'form', 'paging'], function(template, moment) {


        template.helper('format_date', function(date) {
            return moment(date).format('YYYY-MM-DD');
        });

        var form = $('#site_form'),
            nav = $('#nav_tools');

        // 绑定表单
        form.gform({
            url: config.host + 'site/list',
            onSuccess: function(r) {

                if (!r || r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新页面重试');
                    return false;

                }
            }
        });


    });
});