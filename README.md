#  Sistema de Restaurante

> Sistema de gestión de pedidos para restaurantes con autenticación JWT, control por roles y comunicación en tiempo real entre mozo y chef.

![HTML](https://img.shields.io/badge/HTML-66.5%25-E34F26?style=flat-square&logo=html5&logoColor=white)
![C#](https://img.shields.io/badge/C%23-23.6%25-239120?style=flat-square&logo=c-sharp&logoColor=white)
![CSS](https://img.shields.io/badge/CSS-9.1%25-1572B6?style=flat-square&logo=css3&logoColor=white)
![JavaScript](https://img.shields.io/badge/JavaScript-0.8%25-F7DF1E?style=flat-square&logo=javascript&logoColor=black)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?style=flat-square&logo=jsonwebtokens&logoColor=white)
![H2](https://img.shields.io/badge/DB-H2-003545?style=flat-square)

---

##  Descripción

Sistema web y de escritorio para la gestión integral de un restaurante. Permite que el **mozo** registre pedidos de las mesas y los envíe al **chef** para su preparación, todo en tiempo real. El sistema cuenta con autenticación JWT y protección de rutas según el rol del usuario.

---

## ✨ Funcionalidades principales

-  **Autenticación JWT** con protección de rutas por rol
-  **3 roles de usuario:** Administrador, Mozo y Chef
-  **Envío de pedidos** del mozo al chef en tiempo real
-  **Panel de Administrador** con CRUD completo para:
  - Usuarios
  - Empleados
  - Turnos
  - Carta del menú
-  **Vista del Chef** para visualizar y gestionar pedidos entrantes
-  **Vista del Mozo** para registrar y enviar órdenes por mesa

---

##  Arquitectura del proyecto

```
Sistema-de-restaurante/
├── AppFinal/           # Frontend en C# (WinForms/WPF)
│   ├── Servicios/      # Llamadas a la API REST
│   └── Roles/          # Gestión de roles y token JWT
└── FinalDesarrollo/    # Backend / API REST
```

---

##  Tecnologías utilizadas

| Capa | Tecnología |
|------|-----------|
| Frontend de escritorio | C# (WinForms / WPF) |
| Interfaz web | HTML, CSS, JavaScript |
| Autenticación | JSON Web Tokens (JWT) |
| Base de datos | H2 (embebida) |
| Comunicación | API REST |

---

##  Cómo ejecutar el proyecto

### Requisitos previos

- .NET Framework o .NET SDK instalado
- Java (para la base de datos H2, si aplica)
- Navegador web moderno

### Pasos

1. Clona el repositorio:
   ```bash
   git clone https://github.com/zasesinox242/Sistema-de-restaurante.git
   cd Sistema-de-restaurante
   ```

2. Inicia el backend (API REST):
   ```bash
   cd FinalDesarrollo
   ```

3. Abre la aplicación de escritorio:
   ```bash
   cd AppFinal
   # Abre el proyecto en Visual Studio y ejecuta
   ```

---

##  Roles y permisos

| Rol | Permisos |
|-----|---------|
| **Admin** | CRUD de usuarios, empleados, turnos y carta |
| **Mozo** | Registrar y enviar pedidos al chef |
| **Chef** | Ver y gestionar pedidos entrantes |


---

##  Licencia

Este proyecto es de uso educativo y está disponible de forma abierta.

---
