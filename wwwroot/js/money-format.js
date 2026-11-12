
document.addEventListener("DOMContentLoaded", function () {

    // Định dạng số tiền: 1500000 -> 1.500.000
    function formatVND(value) {
        let number = String(value ?? "").replace(/\D/g, "");

        if (!number) return "";

        number = number.replace(/^0+(?=\d)/, "");

        return number.replace(/\B(?=(\d{3})+(?!\d))/g, ".");
    }

    // Định dạng các ô nhập tiền
    document.querySelectorAll(".vnd-input").forEach(function (input) {

        // Ô ẩn dùng để gửi dữ liệu số về Controller
        const hiddenId = input.dataset.target;
        const hiddenInput = document.getElementById(hiddenId);

        function updateValue() {
            const rawValue = input.value.replace(/\D/g, "");

            input.value = formatVND(rawValue);

            if (hiddenInput) {
                hiddenInput.value = rawValue;
            }
        }

        input.addEventListener("input", updateValue);

        updateValue();
    });

    // Định dạng các vị trí hiển thị tiền
    document.querySelectorAll(".vnd-display").forEach(function (element) {

        const rawValue = element.dataset.value;

        if (rawValue !== undefined) {
            element.textContent = formatVND(rawValue) + "đ";
        }
    });

});
