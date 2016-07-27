(function () {

    var URL = "../../js/vendor/umeditor/";

    window.UMEDITOR_CONFIG = {

        //为编辑器实例添加一个路径，这个不能被注释
        UMEDITOR_HOME_URL: URL

        //图片上传配置区
        , imageUrl: URL + "net/imageUp.ashx"             //图片上传提交地址
        , imagePath: URL + "net/"                     //图片修正地址，引用了fixedImagePath,如有特殊需求，可自行配置
        , imageFieldName: "upfile"                   //图片数据的key,若此处修改，需要在后台对应文件修改对应参数

        , toolbar: [
            'undo redo | bold italic underline strikethrough | forecolor backcolor ',
            '| insertorderedlist insertunorderedlist | fontfamily fontsize ',
            '| justifyleft justifycenter justifyright | link image video ',
            '| horizontal preview fullscreen'
        ]

        , lang: "zh-cn"
        , langPath: URL + "lang/"
        , theme: 'default'
        , themePath: URL + "themes/"
        , charset: "utf-8"
        , initialFrameWidth: 500 //初始化编辑器宽度,默认500
        , initialFrameHeight: 500 //初始化编辑器高度,默认500
        , focus: true //初始化时，是否让编辑器获得焦点true或false
        , autoClearEmptyNode: false //getContent时，是否删除空的inlineElement节点（包括嵌套的情况）

        , zIndex: 900     //编辑器层级的基数,默认是900

        //浮动时工具栏距离浏览器顶部的高度，用于某些具有固定头部的页面
        , topOffset: 30
    };
})();
