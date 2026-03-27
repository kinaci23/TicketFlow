import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRegisterDto } from '../../../core/models/auth.model';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent implements OnInit {
  // Register Form
  registerForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  // Submit Register
  onSubmit(): void {
    if (this.registerForm.invalid) {
      return;
    }

    const registerData: UserRegisterDto = {
      username: this.registerForm.value.username,
      email: this.registerForm.value.email,
      password: this.registerForm.value.password
    };

    this.authService.register(registerData).subscribe({
      next: (response) => {
        alert("Kayıt Başarılı! Giriş sayfasına yönlendiriliyorsunuz...");
        this.router.navigate(['/auth/login']);
      },
      error: (err) => {
        console.error("Kayıt Hatası:", err);
        alert("Kayıt Başarısız! Girdiğiniz bilgileri kontrol edin (Bu mail adresi daha önceden eklendi veya bilgiler eksik olabilir).");
      }
    });
  }
}