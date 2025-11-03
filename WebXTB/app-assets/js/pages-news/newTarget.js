window.onload = function () {

   
};
function startSearchNewTarget() {
    const logBox = document.getElementById("logBox");
    logBox.value = ""; // Xóa log cũ

    const evt = new EventSource('/Home/SearchNewTargetStream');

    evt.onmessage = function (e) {
        try {
            const data = JSON.parse(e.data);
            if (data.done) {
                logBox.value += "\n✅ Hoàn tất!\n";
                evt.close();
                return;
            }

            let line = "";
            if (data.status) line += "📡 " + data.status;
            if (data.query) line += " → " + data.query;

            logBox.value += line + "\n";
            logBox.scrollTop = logBox.scrollHeight; // tự cuộn xuống cuối
        } catch {
            logBox.value += e.data + "\n";
        }
    };

    evt.onerror = function () {
        logBox.value += "\n❌ Mất kết nối hoặc server ngắt stream.\n";
        evt.close();
    };
}
