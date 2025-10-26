window.onload = function () {
    const today = new Date();
    const formattedDate = formatDate(today);
    document.getElementById('tuNgay').value = formattedDate;
    document.getElementById('denNgay').value = formattedDate;
    getDataByButtonSearch();
};
function formatDate(date) {
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Tháng bắt đầu từ 0
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
}
function getDataByButtonSearch() {
    showSpinner();
    var ngayNhanTu = document.getElementById("tuNgay").value;
    var ngayNhanDen = document.getElementById("denNgay").value;
    var source = document.getElementById("source").value;
    var dt_table = $('#datatables-newsviewer');
    dt_table.DataTable().destroy();
    var dt_ajax = dt_table.dataTable({
        processing: true,
        dom:
            '<"d-flex justify-content-between align-items-center mx-0 row"<"col-sm-12 col-md-6"><"col-sm-12 col-md-6">>t<"d-flex justify-content-between mx-0 row"<"col-sm-12 col-md-6"i><"col-sm-12 col-md-6"p>>',
        ajax: {
            url: '/Home/GetDataNewsViewer',
            type: 'GET',
            data: {
                tuNgay: ngayNhanTu,
                denNgay: ngayNhanDen,
                source: source,
                mainKeyword: '',
                combineKeyword: '',
                expectKeyword: ''
            },
            complete: function () {
                // Ẩn spinner sau khi xử lý xong
                hideSpinner();
                feather.replace();

            }

        },
        columns: [
            {
                data: 'PostedDate'
            },
            {
                data: null,
                render: function (data, type, row) {
                    // Dòng 1: tiêu đề
                    // Dòng 2: URL in nghiêng và màu khác
                    return `
                    <div style="font-weight:600; color: blue;">${row.Title}</div>
                    <div style="color: green; font-size:0.8em;">
                        <a href="${row.URL}" target="_blank" style="color:green; font-weight:600">${row.URL}</a>
                    </div>
                    <div>
                        <span>${row.SiteAddress}</span>
                    </div>
                `;
                }
            }
        ],
        order: [0, "desc"],
        language: {
            paginate: {
                // remove previous & next text from pagination
                previous: '&nbsp;',
                next: '&nbsp;'
            }
        },
        createdRow: function (row, data, dataIndex) {


            // Thêm sự kiện click
            $(row).on('click', function (event) {
                $('#cardTitle').text(data.Title);
                $('#cardURL')
                    .attr('href', data.URL || '#')
                    .text(data.URL || '(Không có URL)');
                $('#cardContent').val(data.NewsContent || '(Không có nội dung)');
                const textToCheck = (data.URL || '') + '\n' + (data.NewsContent || '');
                const youtubeId = extractYouTubeId(textToCheck);

                if (youtubeId) {
                    // Có video → load iframe trước rồi bật tab
                    const videoSrc = 'https://www.youtube.com/embed/' + youtubeId + '?rel=0';
                    $('#youtubeFrame').attr('src', videoSrc);
                    const videoUrl = getFirstYouTubeUrl(textToCheck);
                    $('#videoURL')
                        .attr('href', videoUrl)
                        .text(videoUrl || '(Không có URL video)');

                    // Bật tab Video
                    const videoTab = document.querySelector('#videoTab');
                    if (videoTab) new bootstrap.Tab(videoTab).show();
                    $('#textTab').removeClass('show active');
                    $('#videoTab').addClass('show active');
                } else {
                    // Không có video → xóa src và bật tab nội dung
                    $('#youtubeFrame').attr('src', '');
                    const textTab = document.querySelector('#textTab');
                    if (textTab) new bootstrap.Tab(textTab).show();
                    $('#videoTab').removeClass('show active');
                    $('#textTab').addClass('show active');
                }
                $('tr.selected-row').removeClass('selected-row');
                $(row).addClass('selected-row'); // Chỉ chọn hàng hiện tại

            });
            // Thêm sự kiện double click
        }
    });

    // ✅ Hàm lấy ID video YouTube từ URL
    function extractYouTubeId(text) {
        if (!text) return null;
        var match = text.match(/(?:youtube\.com\/(?:watch\?v=|embed\/|v\/)|youtu\.be\/)([A-Za-z0-9_-]{11})/);
        return match ? match[1] : null;
    }
    function getFirstYouTubeUrl(text) {
        if (!text) return null;
        const match = text.match(/https?:\/\/(?:www\.)?(?:youtube\.com\/(?:watch\?v=[A-Za-z0-9_-]{11}|embed\/[A-Za-z0-9_-]{11}|v\/[A-Za-z0-9_-]{11})|youtu\.be\/[A-Za-z0-9_-]{11})/);
        return match ? match[0] : null;
    }
}
