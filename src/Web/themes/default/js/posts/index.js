define(['../common'], function () {

    //更新html
    document.title = '我创建的文章 - 站群管理';

    var form = $('#post_form');

    require(['art-template', 'moment', 'select2', 'form',], function (template, moment) {

        template.helper('format_date', function (date) {
            return moment(date).format('YYYY-MM-DD');
        });

        $('select', form).select2({
            minimumResultsForSearch: -1,
            allowClear: true
        });

        form.ajaxForm({
            url: config.host + 'posts/list',
            type: 'POST',
            success: function (r) {

                if (!r || r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新后尝试');
                    return false;
                }

                //更新总数
                $('#total_count').html(r.total_count);

                var table = template('post_table', r);

                $('#table_container', form).html($(table).gtable());
            }
        });

        $('button:first', form).on('click', function () {

            if (form.find('input[name=siteId]').length == 0) {
                form.append('<input type="hidden" name="siteId" value="' + util.get_query('siteId') + '" />');
            }

            form.submit();
        }).trigger('click');
    });

    var bind_table = function (table) {

    };
});