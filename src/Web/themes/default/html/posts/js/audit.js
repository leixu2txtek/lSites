define(['../../../js/common'], function() {

    document.title = '待审核的文章 - 站群管理';

    require(['template', 'moment', 'select2', 'form', 'paging', 'MDialog'], function(template, moment) {

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

        // 绑定表单
        form.gform({
            url: config.host + 'posts/audit',
            onSuccess: function(r) {

                if (!r || r.code < 0) {
                    alert(r.msg || '发生未知错误，请刷新尝试');
                    return false;
                }

                // 更新
                $('#trash_count').html(r.paging.total_count);
                $('#trashtable_container', form).html(template('trash_table', r));

                var table = $('table', form).gtable();

                // 审核通过
                $('.q-approved', table).on('click', function() {

                    var siteId = util.get_query('siteId'),
                        id = $(this).data('id'),
                        dlg = undefined,
                        audit = function(pass) {

                            $.post(config.host + 'posts/update_audit', {
                                ids: [id],
                                siteId: siteId,
                                pass: pass
                            }, function(r) {

                                if (!r || r.code < 0) {
                                    alert(r.msg || '发生未知错误，请刷新尝试');
                                    return false;
                                }

                                dlg.close();
                                alert('成功审核文章');
                                form.submit();

                            }, 'json');
                        };

                    dlg = $M({
                        content: '<p>是否通过审核</p>',
                        ok: function() {

                            audit(1);
                        },
                        okVal: '审核通过',
                        cancel: function() {

                            audit(0);
                        },
                        cancelVal: '审核不通过',

                    });
                });



                // 绑定分页
                $('.x-paging-container', form).paging(r.paging);
            },

            callback: function(form) {
                form.submit();
            }

        });
    });
});