window.onload = function () {

    loadYouTubeMonitor();

    // 🔁 Tự động load lại sau mỗi 30 giây
    setInterval(loadYouTubeMonitor, 15000);
};
function loadYouTubeMonitor() {
    $.ajax({
        url: '/Home/GetYouTubeMonitor',
        method: 'GET',
        success: function (res) {
            if (res.success && Array.isArray(res.data)) {
                renderYouTubeGrid(res.data);
            }
        },
        error: function () {
            console.error('Không tải được danh sách video.');
        }
    });
}
function extractYouTubeId(url) {
    if (!url) return null;
    const regex = /(?:youtu\.be\/|youtube\.com\/(?:watch\?v=|embed\/|v\/|shorts\/))([^?&"'>]+)/;
    const match = url.match(regex);
    return match ? match[1] : null;
}

function createLazyYouTube(youtubeId, title, date) {
    const card = document.createElement('div');
    card.className = 'video-card';

    const thumbUrl = `https://img.youtube.com/vi/${youtubeId}/hqdefault.jpg`;

    const { channel, title2 } = splitTitle(title);
    card.innerHTML = `
        <div class="lazy-yt" data-id="${youtubeId}">
            <img src="${thumbUrl}" alt="${title2}">
        </div>
        <div class="video-title">${title2}</div>
        <div class="video-date">${channel} - ${date}</div>
    `;



    card.querySelector('.lazy-yt').addEventListener('click', function () {
        const iframe = document.createElement('iframe');
        iframe.src = `https://www.youtube-nocookie.com/embed/${youtubeId}?autoplay=0`;
        iframe.setAttribute("frameborder", "0");
        iframe.setAttribute(
            "allow",
            "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; fullscreen"
        );
        iframe.allowFullscreen = true;
        this.replaceWith(iframe);
    });

    return card;
}
function splitTitle(fullTitle) {
    const indexTemp = fullTitle.indexOf(" - ");
    let channel = "";
    let title2 = fullTitle;

    if (indexTemp > 0) {
        channel = fullTitle.substring(0, indexTemp);
        title2 = fullTitle.substring(indexTemp + 3);
    }
    return { channel, title2 };
}
function renderYouTubeGrid(videos) {
    const container = document.getElementById('youtubeContainer');
    container.innerHTML = '';

    const shuffled = videos.sort(() => 0.5 - Math.random()).slice(0, 15); // ✅ 15 video

    // ✅ 3 hàng, 5 cột
    for (let i = 0; i < 3; i++) {
        const row = document.createElement('div');
        row.className = 'row g-3';

        for (let j = 0; j < 5; j++) {
            const index = i * 5 + j;
            const video = shuffled[index];
            if (!video) continue;

            const youtubeId = extractYouTubeId(video.URL);
            if (!youtubeId) continue;

            const col = document.createElement('div');
            col.className = 'col-5th';

            const card = createLazyYouTube(youtubeId, video.Title, video.PostedDate);
            col.appendChild(card);
            row.appendChild(col);
        }

        container.appendChild(row);
    }
}