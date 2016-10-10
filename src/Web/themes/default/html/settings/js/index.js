define(['../../../js/common'], function () {

    document.title = '站点信息 - 站群管理';

    require(['template', 'form', 'select2', 'MDialog'], function (template) {

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

            r = handleException(r);

            if (!r) return false;
            if (r.code < 0) {

                alert(r.msg || '发生未知错误，请刷新后尝试');
                return false;
            }

            form = $(template('setting_form', r.data));

            $('[name=theme]', form).select2({
                minimumResultsForSearch: -1
            }).val($('[name=theme]', form).data('selected')).trigger('change');

            $('.btn_preview', form).on('click', function () {

                var url = $('[name=logo]', form).data('file');

                if (url.length == 0) {

                    alert('图片地址不正确，预览失败');
                    return false;
                }

                $M({
                    title: '预览',
                    content: '<img title="LOGO 预览" src="' + url + '"></img',
                    lock: true,
                    position: '50 % 50 %',
                    cancel: false,
                    cancelVal: '取消'
                });

                return false;
            });

            form.gform({
                url: config.host + 'site/save',
                beforeSubmit: function () {

                    // 验证数据是否正确

                    var title = $('[name=title]', form).val(),
                        domain = $('[name=domain]', form).val(),
                        keyWords = $('[name=keyWords]', form).val(),
                        logo = $('[name=logo]', form).val();

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

                    var extension = logo.substr(logo.lastIndexOf('.') + 1).toLocaleLowerCase();
                    if (logo && extension != 'jpg' && extension != 'gif' && extension != 'png') {

                        $('[name=logo]', form).parent().addClass('has-error');
                        alert('LOGO文件只能是小于 1MB 的 JPG、GIF、PNG 图片文件');

                        return false;
                    }
                },
                onSuccess: function (r) {

                    r = handleException(r);

                    if (!r || r.code < 0) {

                        alert(r.msg || '发生未知错误，请刷新后尝试');
                        return false;
                    }

                    // 保存成功后
                    alert('保存成功');
                }
            });

            $('#setting_form_container').html(form);
        });
    });
});