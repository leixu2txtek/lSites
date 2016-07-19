define(['../../../js/common'], function() {

    document.title = '回收站 - 站群管理';

    require(['template', 'moment', 'select2', 'form', 'paging'], function(template, moment) {

        var form = $('#trash_form');

        template.helper('format_date', function(date) {
            return moment(date).format('YYYY-MM-DD');
        });

        // select2
        $('select', form).select2({
            minimumResultsForSearch: -1,
            allowClear: true
        });

        if (form.find('input[name=siteId]').length == 0) {
            form.append('<input type="hidden" name="siteId" value="' + util.get_query('siteId') + '" />');
        }

    });
});