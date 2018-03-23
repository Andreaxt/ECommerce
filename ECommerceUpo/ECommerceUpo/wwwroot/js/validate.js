function validateUser()
{
    var redColor = "#d82b2b";
    var email = document.forms["form1"]["email"].value;
    var password = document.forms["form1"]["password"].value;
    var pass2 = document.forms["form1"]["pass2"].value;
    var pattern = /^[A-Za-z0-9\s]+@[A-Za-z0-9\s]+.[A-Za-z]+$/;

   

    if (!pattern.test(email))
    {
        blankFields();
        document.getElementById("email").style.backgroundColor = redColor;
        alert("Username deve essere un indirizzo mail!");

        return false;
    }

    if (password !== pass2)
    {
        blankFields();
        document.getElementById("password").style.backgroundColor = redColor;
        document.getElementById("pass2").style.backgroundColor = redColor;
        alert("Le password non corrispondono!");

        return false;
    }
    
    form1.submit();
}

function blankFields()
{
    var whiteColor = "#ffffffff";
    document.getElementById("email").style.backgroundColor = whiteColor;
    document.getElementById("password").style.backgroundColor = whiteColor;
    document.getElementById("pass2").style.backgroundColor = whiteColor;
}