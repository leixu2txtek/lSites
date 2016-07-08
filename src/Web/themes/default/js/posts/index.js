define(['../common'], function () {

    //更新html
    document.title = '我创建的文章 - 站群管理';

    require(['art-template', 'moment', 'select2', 'form', 'paging'], function (template, moment) {

        var form = $('#post_form');
        template.helper('format_date', function (date) {
            return moment(date).format('YYYY-MM-DD');
        });

        $('select', form).select2({
            minimumResultsForSearch: -1,
            allowClear: true
        });

        if (form.find('input[name=siteId]').length == 0) {
            form.append('<input type="hidden" name="siteId" value="' + util.get_query('siteId') + '" />');
        }

        form.gform({
            url: config.host + 'posts/list',
            onSuccess: function (r) {

                if (!r || r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新后尝试');
                    return false;
                }

                //更新总数
                $('#total_count').html(r.paging.total_count);
                $('#table_container', form).html(template('post_table', r));

                var table = $('table', form).gtable();

                $('.x-paging-container', form).paging(r.paging);
            },
            callback: function (form) {
                form.submit();
            }
        });
    });

    var bind_table = function (table) {

    };
});