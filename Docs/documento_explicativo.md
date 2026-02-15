# Documento Explicativo: API REST para Gestión de Familias y Alumnos

## Introducción
Este documento describe los requisitos y especificaciones para el desarrollo de una API REST destinada a gestionar las familias y alumnos de una asociación de padres de alumnos (APA). La aplicación permitirá administrar datos relacionados con familias, alumnos, usuarios, anotaciones y logs, utilizando tecnologías modernas para asegurar seguridad, escalabilidad y mantenibilidad.

## Arquitectura y Tecnologías
- **Framework**: La aplicación se desarrolla con .NET Core, implementando una arquitectura de capas para separar responsabilidades (por ejemplo, capa de presentación, lógica de negocio y acceso a datos).
- **Base de Datos**: Los datos se almacenan en SQL Server.
- **Logging**: Se utiliza Serilog para registrar eventos y errores de manera estructurada y flexible.
- **Autenticación y Autorización**: Se emplean JWT (JSON Web Tokens) para autenticar usuarios. Se implementa un sistema de políticas de autorización basado en roles para controlar el acceso a operaciones.

## Usuarios y Roles
- **Tipos de Usuarios**: Administradores y usuarios normales. Es obligatorio registrarse para usar la aplicación.
- **Roles y Permisos**:
  - **Administradores**: Pueden realizar todas las operaciones, incluyendo la gestión de familias, alumnos, usuarios y anotaciones. También tienen acceso a los logs de la aplicación.
  - **Usuarios Normales**: Pueden tener roles de consulta o gestión.
    - **consulta_familias**: Solo pueden consultar datos de familias.
    - **gestion_familias**: Pueden realizar todas las operaciones sobre familias y anotaciones, pero no gestionar usuarios.
- La diferencia clave entre administradores y usuarios con `gestion_familias` es que los administradores pueden gestionar usuarios, mientras que los otros no.

## Gestión de Familias y Alumnos
- **Entidades Principales**:
  - **Familias**: Centro de la aplicación. Cada familia puede tener uno o más alumnos asociados, o ninguno. Una familia puede estar asociada o no a la APA.
  - **Alumnos**: Un alumno solo puede estar vinculado a una familia (relación uno a uno), pero puede no estar asociado a ninguna.
- **Relación**: La situación de un alumno respecto a la APA, Mutual o beneficiario de la Mutual es la misma que la de su familia. Si un alumno no pertenece a una familia, no puede ser socio de la APA, Mutual o beneficiario.

## Clasificación de Familias
- **Familias Asociadas**: Pertenecen a la APA y tienen estados: activa, inactiva o temporal (basado en el pago de cuotas).
- **Familias No Asociadas**: No pertenecen a la APA, sin estado asociado.
- **Familias Socias de la Mutual**: Son socias de la APA y de la Mutual.
- **Familias Beneficiarias de la Mutual**: Socias de APA y Mutual, pero exentas de pagar cuotas.

## Formas de Pago y Estados
- **Formas de Pago**:
  - **Efectivo**: Pago en efectivo o ingreso bancario. No requiere número de cuenta.
  - **Domiciliación Bancaria**: Cobro automático. Requiere obligatoriamente un número de cuenta bancaria.
- **Estados**: Tanto para APA como para Mutual, los estados son activo, inactivo o temporal. Se registra la fecha de establecimiento de cada estado.

## Anotaciones y Categorías
- **Anotaciones**: Cada familia puede tener anotaciones como histórico (ejemplos: pagos, cambios de forma de pago, cambios de estado). Sirven para rastrear eventos relevantes.
- **Estructura de Anotaciones**:
  - `id`: Identificador único.
  - `id de la familia`: Referencia a la familia asociada.
  - `texto`: Contenido de la anotación.
  - `fecha`: Fecha de creación.
  - `categoría`: Categoría asignada para clasificación.
- **Categorías**:
  - **Del Sistema**: No se pueden eliminar, solo utilizar.
  - **Personalizadas**: Se pueden eliminar o bloquear.
  - **Reglas**: Una categoría asociada a anotaciones no se puede eliminar, pero sí bloquear para evitar uso futuro. Si no está asociada, se puede eliminar o bloquear.

## Estructura de Datos
- **Familia**:
  - `id`: Identificador único.
  - `nombre`: Nombre de la familia.
  - `número de socio`: Número de socio (si aplica).
  - `dirección`: Dirección física.
  - `email`: Correo electrónico.
  - `teléfono`: Número de teléfono.
  - `observaciones`: Notas adicionales.
  - `forma de pago`: Efectivo o domiciliación bancaria.
  - `número de cuenta bancaria`: Obligatorio solo si forma de pago es domiciliación.
  - `indicador de si es socio o no del apa`: Booleano.
  - `estado como socio del apa`: Activo, inactivo o temporal.
  - `fecha del estado`: Fecha de establecimiento del estado APA.
  - `indicador de si es socio o no de la Mutual`: Booleano.
  - `estado como socio de la Mutual`: Activo, inactivo o temporal.
  - `fecha del estado`: Fecha de establecimiento del estado Mutual.
  - `indicador de si es beneficiario o no de la Mutual`: Booleano.
  - `estado como beneficiario de la Mutual`: Activo, inactivo o temporal.
  - `fecha del estado`: Fecha de establecimiento del estado beneficiario.
- **Alumno**:
  - `id`: Identificador único.
  - `nombre`: Nombre del alumno.
  - `apellidos`: Apellidos del alumno.
  - `observaciones`: Notas adicionales.
  - `autoriza redes`: Booleano, registrado por alumno individual.
  - `curso`: Curso y clase asignados (opcional; puede estar vacío).

Este documento proporciona una visión general de los requisitos. Para implementaciones detalladas, consulta la documentación técnica del proyecto.