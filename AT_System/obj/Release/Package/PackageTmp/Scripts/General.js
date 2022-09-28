function chkTotalMax(qty, obj, type) {
    var id = obj.attr("id").substring(4);
    //var qty = parseInt($(this).val());
    if (isNaN(qty)) { qty = 0; }
    var totalPrice = qty * parseFloat($("#price_" + id).val().toString().replace(/,/g, ""));
    var totalPrice_Old = parseFloat($("#total_" + id).val().toString().replace(/,/g, ""))
    var totalSum = parseFloat($("#TotalSum").val().toString().replace(/,/g, ""));
    totalSum = totalSum - totalPrice_Old;

    totalSum = totalSum + totalPrice;
    var totalmax = parseInt("@ViewBag.TotalMax");
    if (totalSum > totalmax) {
        alert("ยอดรวมเกิน 5,000,000");
        if (type == "keyup") {
            qty = qty.toString();
            obj.val(qty.substring(0, qty.length - 1));
        }
        return false;
    }
    $("#total_" + id).val(totalPrice.toFixed(2).toString().replace(/,/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ","));
    $("#TotalSum").val(totalSum.toFixed(2).toString().replace(/,/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ","));
}