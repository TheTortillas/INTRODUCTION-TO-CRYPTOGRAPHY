import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { CommonModule } from '@angular/common';
import { UserManagementService } from '../../../core/services/user-management.service';
import { UserSignUp } from '../../../interfaces/SignUp.interface';

import Swal from 'sweetalert2';

@Component({
  selector: 'app-sign-up',
  standalone: true,
  imports: [
    MatIconModule,
    MatButtonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDividerModule,
    RouterLink,
    CommonModule,
    HttpClientModule,
  ],
  templateUrl: './sign-up.component.html',
  styleUrl: './sign-up.component.scss',
})
export class SignUpComponent {
  form: FormGroup;
  private hidePassword = true;

  constructor(
    private userManagementService: UserManagementService,
    private router: Router
  ) {
    this.form = new FormGroup({
      firstname: new FormControl('', [Validators.required]),
      lastname: new FormControl('', [Validators.required]),
      secondlastname: new FormControl('', [Validators.required]),
      email: new FormControl('', [Validators.required, Validators.email]),
      password: new FormControl('', [
        Validators.required,
        Validators.minLength(6),
      ]),
    });
  }

  hide() {
    return this.hidePassword;
  }

  clickEvent(event: MouseEvent) {
    event.preventDefault();
    this.hidePassword = !this.hidePassword;
  }

  updateEmailErrorMessage() {
    const emailControl = this.form.get('email');
    if (emailControl?.hasError('required')) {
      return 'El correo electrónico es requerido';
    }
    if (emailControl?.hasError('email')) {
      return 'Ingresa un correo electrónico válido';
    }
    return '';
  }

  signUp() {
    if (this.form.valid) {
      const userData: UserSignUp = {
        firstname: this.form.get('firstname')?.value,
        lastname: this.form.get('lastname')?.value,
        secondlastname: this.form.get('secondlastname')?.value,
        email: this.form.get('email')?.value,
        password: this.form.get('password')?.value,
      };

      this.userManagementService.signUp(userData).subscribe({
        next: () => {
          Swal.fire({
            icon: 'success',
            title: '¡Registro exitoso!',
            text: 'Tu cuenta ha sido creada correctamente',
            showConfirmButton: false,
            timer: 1500,
          });
          this.router.navigate(['/auth/sign-in']);
        },
        error: (error) => {
          Swal.fire({
            icon: 'error',
            title: 'Error',
            text: error.error.message || 'Error al registrar usuario',
          });
        },
      });
    } else {
      Object.keys(this.form.controls).forEach((key) => {
        const control = this.form.get(key);
        control?.markAsTouched();
      });
    }
  }
}
