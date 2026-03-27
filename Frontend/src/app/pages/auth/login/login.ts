import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { UserLoginDto } from '../../../core/models/auth.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent implements OnInit {
  // Login Form
  loginForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  // Submit Login
  onSubmit(): void {
    if (this.loginForm.invalid) {
      return;
    }

    const loginData: UserLoginDto = {
      username: this.loginForm.value.username,
      password: this.loginForm.value.password
    };

    console.log("Backend'e gönderiliyor...", loginData);

    this.authService.login(loginData).subscribe({
      next: (res) => {
        localStorage.setItem('token', res.token);

        const decoded = this.authService.getDecodedToken();
        const userRole = decoded ? decoded.role : null;

        if (userRole === 'Admin') {
          this.router.navigate(['/dashboard/all-tickets']);
        } else {
          this.router.navigate(['/dashboard/my-tickets']);
        }
      },
      error: (err) => alert("Giriş başarısız! Lütfen kullanıcı adı ve şifrenizi kontrol edip tekrar deneyin.")
    });
  }
}