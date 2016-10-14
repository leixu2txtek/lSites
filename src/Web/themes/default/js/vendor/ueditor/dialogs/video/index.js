UE.registerUI('video', function (editor, uiName) {

    var dialog = new UE.ui.Dialog({
        iframeUrl: editor.options.UEDITOR_HOME_URL + 'dialogs/video/form.html',
        editor: editor,
        name: uiName,
        title: '上传视频',
        cssRules: 'width:600px;height:150px;'
    });

    return new UE.ui.Button({
        name: 'btn_form_video',
        title: '添加视频',
        cssRules: 'background-position: -321px 101px;',
        onclick: function () {
            dialog.render();
            dialog.open();
        }
    });
});