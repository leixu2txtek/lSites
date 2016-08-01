define(['../../../js/common'], function () {

    document.title = '站点信息 - 站群管理';

    require(['template', 'form', 'select2'], function (template) {

        var siteId = util.get_query('siteId'),
            nav = $('#nav_tools'),
            form = undefined;

        //保存       
        $('.btn_save', nav).on('click', function () {

            form.submit();
            return false;

        });

        $.ajax({
            url: config.host + 'site/detail',
            type: 'POST',
            dataType: 'json',
            data: { id: siteId },
            async: false
        }).done(function (r) {

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
                beforeSubmit: function () {

                    // 验证数据是否正确

                    var title = $('[name=title]', form).val(),
                        domain = $('[name=domain]', form).val(),
                        keyWords = $('[name=keyWords]', form).val();

                    if (title.length == 0) {

                        $('[name=title]', form).parent().addClass('has-error');
                        alert('名称不能为空');

                        return false;
                    }

                    if (domain.length == 0) {

                        $('[name=domain]', form).parent().addClass('has-error');
                        alert('域名不能为空');

                        return false;
                    }

                    if (keyWords.length == 0) {

                        $('[name=keyWords]', form).parent().addClass('has-error');
                        alert('关键字不能为空');

                        return false;
                    }

                },
                onSuccess: function (r) {

                    if (!r || r.code < 0) {

                        alert(r.msg || '发生未知错误，请刷新后尝试');
                        return false;
                    }

                    // 保存成功后

                    alert('已修改添加站点信息');
                    form.submit(); //重新刷新站点列表
                }
            });

            $('#setting_form_container').html(form);
        });
    });
});