mergeInto(LibraryManager.library, {
    OpenNewWindow: function (openUrl) {
        var url = Pointer_stringify(openUrl);
        window.open(url, "TweetWindow");
    }
})
