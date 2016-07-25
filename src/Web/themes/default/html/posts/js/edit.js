define(['../../../js/common'], function () {

    require(['template', 'ztree', 'form', 'select2'], function (template) {

        var id = util.get_query('id');

        document.title = (id ? '编辑文章' : '添加文章') + ' - CMS内容管理系统';

        $('#txt_header').text(id ? '编辑文章' : '添加文章');

        $.ajax({
            url: config.host + 'posts/detail',
            type: 'POST',
            dataType: 'json',
            data: { siteId: util.get_query('siteId'), id: id },
            async: false
        }).done(function (r) {

            if (!r || r.code < 0) {
                alert(r.msg || '发生未知错误，请刷新页面后尝试');

                window.close();
                return false;
            }

            var container = $('#editor_container');

            //构造HTML
            container.html(template('post_edit_form', r));

            //TODO bind events
        });
    });
});