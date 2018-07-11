//import { error } from "util";

const uri = 'api/';
let token = "";

function createCustomer() {
    console.log("create");
    const user = {
        "firstName" : $("#firstName").val(),
        "lastName" : $("#lastName").val(),
        "password": $("#password").val(),
        "email": $("#email").val(),
        "phoneNumber": $("#phone").val()
    }
    const customerUri = uri + "accounts/customers";
    console.log(user);
    $.ajax({
        type: 'POST',
        accepts: 'application/json',
        url: customerUri,
        contentType: 'application/json',
        data: JSON.stringify(user),
        error: function (jqXHR, textStatus, errorThrown) {
            var obj = jqXHR.responseJSON;
            var values = Object.keys(obj).map(function (key) { return obj[key]; });
            $("#response").html(errorThrown + " " + textStatus + "\n" +
                values);
            console.log(errorThrown);
            console.log(textStatus);
            console.log(jqXHR);
        },
        success: function (result) {
            $("#response").html(JSON.stringify(result));
        }
    });
}

function createDriver() {
    console.log("createDriver");
    const user = {
        "firstName": $("#firstNameDriver").val(),
        "lastName": $("#lastNameDriver").val(),
        "password": $("#passwordDriver").val(),
        "email": $("#emailDriver").val(),
        "phoneNumber": $("#phoneDriver").val(),
        "city" : $("#city").val()
    }
    const customerUri = uri + "accounts/drivers";
    console.log(user);
    $.ajax({
        type: 'POST',
        accepts: 'application/json',
        url: customerUri,
        contentType: 'application/json',
        data: JSON.stringify(user),
        error: function (jqXHR, textStatus, errorThrown) {
            var obj = jqXHR.responseJSON;
            var values = Object.keys(obj).map(function (key) { return obj[key]; });
            $("#response").html(errorThrown + " " + textStatus + "\n" +
                values);console.log(errorThrown);
            console.log(jqXHR);
        },
        success: function (result) {
            $("#response").html(JSON.stringify(result));
            console.log(result);
        }
    });
}

function login() {
    console.log("login");
    const credencials = {
        "userName": $("#emailLogin").val(),
        "password": $("#passwordLogin").val()
    }
    const customerUri = uri + "auth/login";

    $.ajax({
        type: 'POST',
        accepts: 'application/json',
        url: customerUri,
        contentType: 'application/json',
        data: JSON.stringify(credencials),
        error: function (jqXHR, textStatus, errorThrown) {
            var obj = jqXHR.responseJSON;
            var values = Object.keys(obj).map(function (key) { return obj[key]; });
            $("#response").html(errorThrown + " " + textStatus + "\n" +
                values);console.log(errorThrown);
            console.log(textStatus);
        },
        success: function (result) {
            $("#response").html("login succeed");
            var res = JSON.parse(result);
            //console.log(res);
            token = res.auth_token;
         //   console.log(token);
        }
    });

}


function testGet() {
    const customerUri = uri + "auth";
    $.ajax({
        type: 'GET',
        accepts: 'application/json',
        url: customerUri,
        headers: { "Authorization": 'Bearer ' + token },
        contentType: 'application/json',
        error: function (jqXHR, textStatus, errorThrown) {
            $("#response").html(errorThrown + " " + textStatus);
            console.log(errorThrown);
            console.log(textStatus);
        },
        success: function (result) {
            $("#response").html(result);
        }
    });
}
