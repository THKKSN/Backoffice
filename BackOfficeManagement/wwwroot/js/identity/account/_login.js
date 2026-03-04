function ShowPassword() {
  const input = document.getElementById("Password");
  const icon = document.getElementById("IconPassword");
  const isPassword = input.type === "password";

  input.type = isPassword ? "text" : "password";
  icon.classList.toggle("fa-lock");
  icon.classList.toggle("fa-unlock");
}
