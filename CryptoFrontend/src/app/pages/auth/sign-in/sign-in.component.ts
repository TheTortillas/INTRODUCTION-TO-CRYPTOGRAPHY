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
import { StorageService } from '../../../core/services/storage.service';
import { UserSignIn } from '../../../interfaces/SignIn.interface';

import Swal from 'sweetalert2';

@Component({
  selector: 'app-sign-in',
  standalone: true,
  imports: [
    MatIconModule,
    MatButtonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    ReactiveFormsModule,
    MatDividerModule,
    RouterLink,
    CommonModule,
    HttpClientModule,
  ],
  templateUrl: './sign-in.component.html',
  styleUrl: './sign-in.component.scss',
})
export class SignInComponent {
  form: FormGroup;
  private hidePassword = true;

  constructor(
    private userManagementService: UserManagementService,
    private storageService: StorageService,
    private router: Router
  ) {
    this.form = new FormGroup({
      username: new FormControl('', [Validators.required, Validators.email]),
      password: new FormControl('', [
        Validators.required,
        Validators.minLength(6),
      ]),
    });
  }

  // Método para manejar visibilidad de contraseña
  hide() {
    return this.hidePassword;
  }

  clickEvent(event: MouseEvent) {
    event.preventDefault();
    this.hidePassword = !this.hidePassword;
  }

  // Método para actualizar mensajes de error
  updateErrorMessage() {
    const usernameControl = this.form.get('username');
    if (usernameControl?.hasError('required')) {
      return 'El correo electrónico es requerido';
    }
    if (usernameControl?.hasError('email')) {
      return 'Ingresa un correo electrónico válido';
    }
    return '';
  }

  errorMessage = () => this.updateErrorMessage();

  // Método para manejar el inicio de sesión
  signIn() {
    if (this.form.valid) {
      const credentials: UserSignIn = {
        email: this.form.get('username')?.value,
        password: this.form.get('password')?.value,
      };

      this.userManagementService.signIn(credentials).subscribe({
        next: (response) => {
          // Guardar el token
          this.storageService.setItem('token', response.token);

          // Mostrar mensaje de éxito
          Swal.fire({
            icon: 'success',
            title: '¡Bienvenido!',
            text: 'Has iniciado sesión correctamente',
            showConfirmButton: false,
            timer: 1500,
          });

          // Redirigir al usuario
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          // Mostrar mensaje de error
          Swal.fire({
            icon: 'error',
            title: 'Error',
            text: error.error.message || 'Error al iniciar sesión',
          });
        },
      });
    } else {
      // Marcar todos los campos como tocados para mostrar errores
      Object.keys(this.form.controls).forEach((key) => {
        const control = this.form.get(key);
        control?.markAsTouched();
      });
    }
  }
}
