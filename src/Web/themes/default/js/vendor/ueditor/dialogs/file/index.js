UE.registerUI('file', function (editor, uiName) {

    var dialog = new UE.ui.Dialog({
        iframeUrl: editor.options.UEDITOR_HOME_URL + 'dialogs/file/form.html',
        editor: editor,
        name: uiName,
        title: '上传附件',
        cssRules: 'width:600px;height:180px;'
    });

    return new UE.ui.Button({
        name: 'btn_form_file',
        title: '添加附件',
        cssRules: 'background-position: -622px 82px;',
        onclick: function () {
            dialog.render();
            dialog.open();
        }
    });
});