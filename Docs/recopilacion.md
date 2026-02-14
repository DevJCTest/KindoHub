
- Se crea una Api Rest para gestionar las familias de una asociación de padres de alumnos con sus respectivos alumnos.
- Los datos de las familias y alumnos se almacenan en una base de datos SqlServer.
- La aplicación se desarrolla utilizando el framework .NET Core y se implementa una arquitectura de capas
- Para la gestión de los logs se utilizará la biblioteca Serilog, que permite registrar eventos y errores de manera estructurada y flexible.

- Los usuarios de la aplicación pueden ser de dos tipos: administradores o usuarios normales.
- Para usar la aplicación es obligatorio tener un usuario registrado.
- Se utilizarán JWT (JSON Web Tokens) para la autenticación y autorización de los usuarios y se implementará un sistema de políticas de autorización para controlar el acceso a las diferentes operaciones según el rol del usuario.

- Los administradores pueden realizar todas las operaciones y los usuarios normales podrán hacer unas u otras en función del rol que tengan asignado.
- La autorización que pueden tener los usuarios normales es de consulta o gestión. Para ello se utilizáran políticas de autorización basadas en roles (consulta_familias y gestion_familias).
- Los usuarios con el rol de consulta_familias solo pueden consultar los datos de las familias, mientras que los usuarios con el rol de gestion_familias pueden realizar todas las operaciones sobre las familias.
- La diferencia entre un usuario que tenga la autorización de gestion_familias y otro que sea administrador está en que el administrador puede gestionar tanto las familias como los usuarios, mientras que el usuario con autorización de gestion_familias solo puede gestionar las familias y no tiene acceso a la gestión de usuarios.
- Los administradores serán los que también puedan ver los logs de la aplicación.

- El centro de la aplicación es la gestión de las familias que, a su vez, pueden tener vinculados a sus respectivos alumnos. 
- Puede darse el caso que en una familia haya varios alumnos y que en otra familia solo haya uno o ninguno.
- Un alumno solo puede estar vinculado a una familia, es decir, no puede haber un alumno que esté asociado a varias familias. Pero si puede haber alumnos que no estén vinculados a ninguna familia.

- Detro de la gestión de las familias se contemplan varios casuíticas:
- Familias asociadas: Son familias que pertenecen a la asociación de padres de alumnos (APA) y que pueden tener diferentes estados (activa, inactiva o temporal) dependiendo de su situación con respecto al pago de la cuota.
- Familias no asociadas: Son familias que no pertenecen a la asociación de padres de alumnos (APA) y, por lo tanto, no tienen un estado asociado.
- Familias socias de la Mutual: Son familias que, además de ser socias del APA, también son socias de la Mutual
- Familias beneficiarias de la Mutual: Son familias que son socias del APA y de la Mutual, pero que no tienen que pagar las correspondientes cuotas de ambas asociaciones.

- Para hacer la clasificación de las familias en función de cómo pagan las cuotas se han definido las siguientes formas:
- Efectivo: Pagan en efectivo o hacen un ingreso en la cuenta bancaria del APA.
- Domiciliación bancaria: Se les cobra automáticamente la cuota a través de una domiciliación bancaria.

- Si una familia tiene la forma de pago domiciliación bancaria, es obligatorio que tenga un número de cuenta bancaria asociado, mientras que si la forma de pago es efectivo, no tendrá asignada una número de cuenta bancario.
- El estado de las familias, en cuanto a la Mutual, también puede ser activo, inactivo o temporal.
- Al margen del estado, se registrará la fecha en la que se estableció dicho estado. Tanto como asociado del Apa, de la Mutual o de beneficiario de la Mutual.
- La situación de los alumnos en cuanto a la pertenencia al Apa, la Mutual o ser beneficiario de la Mutual, es la misma que la de las familias. Si un alumno no está asociado a ninguna familia, no podrá ser asociado al Apa, a la Mutual o ser beneficiario de la Mutual. En cambio, si un alumno está asociado a una familia que es socia del Apa, también podrá ser socio del Apa. Lo mismo ocurre con la Mutual y con ser beneficiario de la Mutual.


- Los datos de la familia serán los siguientes: id, nombre, número de socio, dirección, email, teléfono, observaciones, forma de pago , número de cuenta bancaria, indicador de si es socio o no del apa, estado como socio del apa, fecha del estado,
- indicador de si es socio o no de la Mutual, estado como socio de la Mutual, fecha del estado, indicador de si es beneficiario o no de la Mutual, estado como beneficiario de la Mutual y fecha del estado.
- Los datos de los alumnos serán los siguientes: id, nombre, apellidos, observaciones, autoriza redes, curso.

- el registro de si un alumno autoriza redes o no es por alumno individual, no por familia.
- Cada alumno puede tener asignado el curso y clase al que pertenece, pero no es obligatorio. Es decir, puede haber alumnos sin asignar curso y clase.

- Cada familia puede tener registradas anotaciones, que servirán un poco como histórico de la familia. Estas anotaciones pueden ser de cualquier tipo, como por ejemplo: "La familia ha pagado la cuota del mes de septiembre", "La familia ha cambiado su forma de pago a domiciliación bancaria", "La familia ha dejado de ser socia del APA", etc.
- Cada anotación tendrá un id, el id de la familia a la que corresponde, el texto de la anotación y la fecha en la que se hizo la anotación. Además, cada anotación también tendrá una categoría. De esta forma, se podrá clasificar las anotaciones.
- Las categorías de las anotaciones pueden ser básicamente de dos tipos: categorías del sistema (se pueden utilizar pero no se pueden eliminar) y categorías personalizadas (que se pueden eliminar y/o bloquear).
- Una categoría que esté asociada a una anotación no se podrá eliminar, pero sí se podrá bloquear para que no se pueda utilizar en nuevas anotaciones. En cambio, una categoría que no esté asociada a ninguna anotación sí se podrá eliminar o bloquear.

- Una familia que está asociada al Apa puede ener o no un número de socio. Esto se debe a que para tener un número de socio, la familia tiene que haberlo solicitado. No puede haber más de una familia con el mismo número de socio.
- 