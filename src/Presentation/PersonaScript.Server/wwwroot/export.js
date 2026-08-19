window.exportHelper = {
    copyToClipboard: async function (text) {
        try {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                await navigator.clipboard.writeText(text);
                return true;
            } else {
                var textArea = document.createElement("textarea");
                textArea.value = text;
                textArea.style.position = "fixed";
                textArea.style.left = "-999999px";
                document.body.appendChild(textArea);
                textArea.focus();
                textArea.select();
                var successful = document.execCommand("copy");
                document.body.removeChild(textArea);
                return successful;
            }
        } catch (err) {
            console.error("Erro ao copiar para clipboard:", err);
            return false;
        }
    },

    downloadFile: function (filename, content, mimeType) {
        var blob = new Blob([content], { type: mimeType || "text/plain;charset=utf-8" });
        var url = URL.createObjectURL(blob);
        var a = document.createElement("a");
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },

    printPage: function () {
        window.print();
    }
};
