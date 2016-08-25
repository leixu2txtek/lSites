/**
 * ueditor完整配置项
 * 可以在这里配置整个编辑器的特性
 */
/**************************提示********************************
 * 所有被注释的配置项均为UEditor默认值。
 * 修改默认配置请首先确保已经完全明确该参数的真实用途。
 * 主要有两种修改方案，一种是取消此处注释，然后修改成对应参数；另一种是在实例化编辑器时传入对应参数。
 * 当升级编辑器时，可直接使用旧版配置文件替换新版配置文件,不用担心旧版配置文件中因缺少新功能所需的参数而导致脚本报错。
 **************************提示********************************/
window.UEDITOR_HOME_URL = "../../js/vendor/ueditor/";

(function () {

    /**
     * 编辑器资源文件根路径。它所表示的含义是：以编辑器实例化页面为当前路径，指向编辑器资源文件（即dialog等文件夹）的路径。
     * 鉴于很多同学在使用编辑器的时候出现的种种路径问题，此处强烈建议大家使用"相对于网站根目录的相对路径"进行配置。
     * "相对于网站根目录的相对路径"也就是以斜杠开头的形如"/myProject/ueditor/"这样的路径。
     * 如果站点中有多个不在同一层级的页面需要实例化编辑器，且引用了同一UEditor的时候，此处的URL可能不适用于每个页面的编辑器。
     * 因此，UEditor提供了针对不同页面的编辑器可单独配置的根路径，具体来说，在需要实例化编辑器的页面最顶部写上如下代码即可。当然，需要令此处的URL等于对应的配置。
     * window.UEDITOR_HOME_URL = "/xxxx/xxxx/";
     */
    var URL = window.UEDITOR_HOME_URL || getUEBasePath();

    /**
     * 配置项主体。注意，此处所有涉及到路径的配置别遗漏URL变量。
     */
    window.UEDITOR_CONFIG = {

        //为编辑器实例添加一个路径，这个不能被注释
        UEDITOR_HOME_URL: URL

        // 服务器统一请求接口路径
        , serverUrl: ''

        //工具栏上的所有的功能按钮和下拉框，可以在new编辑器的实例时选择自己需要的从新定义
        , toolbars: [
            ["fullscreen", "undo", "redo", "bold", "italic", "underline", "fontfamily", "fontsize", "forecolor", "backcolor", "rowspacingtop", "rowspacingbottom", "lineheight", "link", "unlink", "|", "inserttable", "deletetable", "edittable", "insertimage"]
        ]

        , lang: "zh-cn"
        , langPath: URL + "lang/"
        , theme: 'default'
        , themePath: URL + "themes/"

        , zIndex: 100 //编辑器层级的基数,默认是900
        , charset: "utf-8"  //针对getAllHtml方法，会在对应的head标签中增加该编码设置。

        , focus: true //初始化时，是否让编辑器获得焦点true或false

        , autoClearEmptyNode: true //getContent时，是否删除空的inlineElement节点（包括嵌套的情况）

        , enableAutoSave: false    //启用自动保存
        , elementPathEnabled: false //是否启用元素路径，默认是显示

        , autoHeightEnabled: true // 是否自动长高,默认true

        , autoFloatEnabled: true      //是否保持toolbar的位置不动,默认true
        , topOffset: 110            //浮动时工具栏距离浏览器顶部的高度，用于某些具有固定头部的页面

        //, toolbarTopOffset: 100 //编辑器底部距离工具栏高度
    };

    function getUEBasePath(docUrl, confUrl) {

        return getBasePath(docUrl || self.document.URL || self.location.href, confUrl || getConfigFilePath());
    }

    function getConfigFilePath() {

        var configPath = document.getElementsByTagName('script');

        return configPath[configPath.length - 1].src;
    }

    function getBasePath(docUrl, confUrl) {

        var basePath = confUrl;


        if (/^(\/|\\\\)/.test(confUrl)) {

            basePath = /^.+?\w(\/|\\\\)/.exec(docUrl)[0] + confUrl.replace(/^(\/|\\\\)/, '');

        } else if (!/^[a-z]+:/i.test(confUrl)) {

            docUrl = docUrl.split("#")[0].split("?")[0].replace(/[^\\\/]+$/, '');

            basePath = docUrl + "" + confUrl;

        }

        return optimizationPath(basePath);
    }

    function optimizationPath(path) {

        var protocol = /^[a-z]+:\/\//.exec(path)[0],
            tmp = null,
            res = [];

        path = path.replace(protocol, "").split("?")[0].split("#")[0];

        path = path.replace(/\\/g, '/').split(/\//);

        path[path.length - 1] = "";

        while (path.length) {

            if ((tmp = path.shift()) === "..") {
                res.pop();
            } else if (tmp !== ".") {
                res.push(tmp);
            }

        }

        return protocol + res.join("/");
    }

    window.UE = { getUEBasePath: getUEBasePath };
})();