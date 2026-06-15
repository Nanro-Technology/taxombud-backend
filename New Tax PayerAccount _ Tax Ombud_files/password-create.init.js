// Password helpers: toggle visibility, live strength UI, and confirm match handling.
(function () {
  // Toggle visibility buttons
  document.querySelectorAll('form .auth-pass-inputgroup').forEach(function (group) {
    group.querySelectorAll('.password-addon').forEach(function (btn) {
      btn.addEventListener('click', function () {
        var input = group.querySelector('.password-input');
        if (!input) return;
        input.type = input.type === 'password' ? 'text' : 'password';
      });
    });
  });

  var password = document.getElementById('password-input');
  var confirmPassword = document.getElementById('confirm-password-input');

  function validatePassword() {
    if (!password || !confirmPassword) return;
    confirmPassword.setCustomValidity(
      password.value !== confirmPassword.value ? "Passwords don't match" : ''
    );
  }

  if (password && confirmPassword) {
    password.addEventListener('input', validatePassword);
    confirmPassword.addEventListener('input', validatePassword);
    password.addEventListener('change', validatePassword);
    confirmPassword.addEventListener('change', validatePassword);
  }

  // Strength hints
  var letter = document.getElementById('pass-lower');
  var capital = document.getElementById('pass-upper');
  var number = document.getElementById('pass-number');
  var length = document.getElementById('pass-length');
  var meterWrap = document.getElementById('password-contain');

  if (password && meterWrap && letter && capital && number && length) {
    password.onfocus = function () {
      meterWrap.style.display = 'block';
    };
    password.onblur = function () {
      meterWrap.style.display = 'none';
    };
    function checkPasswordStrength() {
      var val = password.value || '';
      if (val.match(/[a-z]/g)) {
        letter.classList.add('valid');
        letter.classList.remove('invalid');
      } else {
        letter.classList.add('invalid');
        letter.classList.remove('valid');
      }
      if (val.match(/[A-Z]/g)) {
        capital.classList.add('valid');
        capital.classList.remove('invalid');
      } else {
        capital.classList.add('invalid');
        capital.classList.remove('valid');
      }
      if (val.match(/[0-9]/g)) {
        number.classList.add('valid');
        number.classList.remove('invalid');
      } else {
        number.classList.add('invalid');
        number.classList.remove('valid');
      }
      if (val.length >= 12) {
        length.classList.add('valid');
        length.classList.remove('invalid');
      } else {
        length.classList.add('invalid');
        length.classList.remove('valid');
      }
    }
    password.onkeyup = checkPasswordStrength;
    password.addEventListener('input', checkPasswordStrength);
  }
})();