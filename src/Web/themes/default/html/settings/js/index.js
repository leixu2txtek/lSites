define(['../../../js/common'], function() {

    document.title = '站点信息 - 站群管理';

    require(['template', 'form', 'select2'], function(template) {

        var siteId = util.get_query('siteId'),
            nav = $('#nav_tools'),
            form = undefined;

        //保存       
        $('.btn_save', nav).on('click', function() {

            form.submit();
            return false;
        });

        $.ajax({
            url: config.host + 'site/detail',
            type: 'POST',
            dataType: 'json',
            data: { id: siteId },
            async: false
        }).done(function(r) {

            if (!r || r.code < 0) {

                alert(r.msg || '发生未知错误，请刷新后尝试');
                return false;
            }

            form = $(template('setting_form', r.data));

            $('[name=theme]', form).select2({
                minimumResultsForSearch: -1
            }).val($('[name=theme]', form).data('selected')).trigger('change');

            form.gform({
                url: config.host + 'site/save',
                beforeSubmit: function() {

                    //TODO 验证数据是否正确
                },
                onSuccess: function(r) {

                    if (!r || r.code < 0) {

                        alert(r.msg || '发生未知错误，请刷新后尝试');
                        return false;
                    }

                    //TODO 保存成功后

                }
            });

            $('#setting_form_container').html(form);
        });
    });
});