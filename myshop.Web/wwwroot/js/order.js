var dtable;
$(document).ready(function() {
    loaddata();
});

function loaddata() {
    dtable = $("#mytable").DataTable({
        "ajax": {
            "url" : "/Admin/Order/GetData"
        },
        "columns": [
            { "data": "id" },
            { "data": "name" },
            { "data": "phoneNumber" },
            { "data": "applicationUser.email" },
            { "data": "orderStatus" },
            { "data": "totalPrice" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <a href = "/Admin/Order/Details?orderid=${data}" class = "btn btn-warning">Details</a>
                           `
                }
            }
        ]
    });
}

function DeleteItem(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "Delete",
                success: function (data) {
                    if (data.success) {
                        dtable.ajax.reload();
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                },
                error: function () {
                    toastr.error("Error deleting product.");
                }
            });
            Swal.fire(
                'Deleted!',
                'Your file has been deleted',
                'success'
            )
        }
    });
}
