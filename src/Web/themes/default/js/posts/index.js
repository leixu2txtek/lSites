define(['../common'], function () {

    //更新html
    document.title = '我创建的文章 - 站群管理';

    var form = $('#post_form');

    require(['select2', 'form'], function () {

        $('select', form).select2({});

        form.ajaxForm({
            url: config.host + 'posts/list',
            type: 'POST',
            success: function (r) {
                debugger;
            }
        });

        $('button:first', form).on('click', function () {

            if (form.find('input[name=siteId]').length == 0) {
                form.append('<input type="hidden" name="siteId" value="' + util.get_query('siteId') + '" />');
            }

            form.submit();
        });
    });
});