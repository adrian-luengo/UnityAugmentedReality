# 🔮 AR Circuit Simulator — Unity & Realidad Aumentada

> Proyecto académico desarrollado en la asignatura de **Construcción y Diseño de Interfaces(CDIG)** de la **Universidad Politécnica de Madrid (UPM)**, que aplica el desarrollo de aplicaciones de **Realidad Aumentada** sobre dispositivos móviles Android mediante el motor **Unity**, el **SDK de Vuforia** y programación en **C#**, simulando circuitos eléctricos en tiempo real sobre el mundo físico.

---

## 📋 Tabla de Contenidos

- [Descripción del Proyecto](#descripción-del-proyecto)
- [Ciclo de Vida del Software](#ciclo-de-vida-del-software)
  - [Fase 1 — Análisis y Requisitos del Sistema AR](#fase-1--análisis-y-requisitos-del-sistema-ar)
  - [Fase 2 — Diseño del Sistema](#fase-2--diseño-del-sistema)
  - [Fase 3 — Implementación en Unity y C#](#fase-3--implementación-en-unity-y-c)
  - [Fase 4 — Pruebas y Validación](#fase-4--pruebas-y-validación)
- [Arquitectura del Sistema](#arquitectura-del-sistema)
- [Componentes del Circuito](#componentes-del-circuito)
- [Algoritmos y Lógica de Negocio](#algoritmos-y-lógica-de-negocio)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Tecnologías y Herramientas](#tecnologías-y-herramientas)
- [Instalación y Configuración](#instalación-y-configuración)
- [Uso de la Aplicación](#uso-de-la-aplicación)

---

## Descripción del Proyecto

**AR Circuit Simulator** es una aplicación móvil de realidad aumentada con propósito educativo que permite a los usuarios construir y validar circuitos eléctricos en el mundo físico. Mediante tarjetas de papel impresas con imágenes específicas que actúan como **marcadores AR**, la cámara del dispositivo los reconoce en tiempo real con el **SDK de Vuforia** y superpone sobre ellas **modelos 3D** de componentes electrónicos reales (bombilla, pila, resistencia, amperímetro, interruptor, tierra).

La aplicación analiza la disposición física de los marcadores en el espacio, determina el orden del circuito por posición, valida si cumple las reglas eléctricas básicas y calcula la **intensidad de corriente mediante la Ley de Ohm**. La bombilla 3D responde visualmente con distintos tonos de color según la intensidad calculada, simulando el brillo de una bombilla real.

El proyecto corresponde a la asignatura de **CDIG (Computación Digital)** y fue desarrollado individualmente, aplicando programación orientada a componentes en Unity, C# avanzado, arquitectura de sistemas AR y principios de física eléctrica.

---

## Ciclo de Vida del Software

### Fase 1 — Análisis y Requisitos del Sistema AR

Antes de implementar, se analizaron los requisitos funcionales del sistema, que en el contexto de AR presentan particularidades distintas al software de escritorio: el sistema debe reaccionar al mundo físico en tiempo real, gestionar la detección y pérdida de marcadores, y mantener un estado coherente ante eventos asíncronos de la cámara.

#### Requisitos funcionales identificados

**Detección AR:**
- Reconocer físicamente tarjetas impresas como marcadores AR mediante la cámara del móvil
- Añadir el objeto 3D correspondiente al circuito cuando el marcador es detectado
- Eliminar el objeto 3D del circuito cuando el marcador pierde el seguimiento
- Soporte de dos bases de datos de marcadores (versión original y versión mejorada con imágenes GPT)

**Gestión del circuito:**
- Ordenar los componentes detectados por posición en el eje X (izquierda a derecha)
- Validar el circuito según 7 reglas eléctricas en secuencia
- Recalcular el estado del circuito cada N segundos de forma automática
- Mostrar el resultado de la validación con color verde (correcto) o rojo (error)

**Simulación eléctrica:**
- Calcular voltaje, resistencia total e intensidad (Ley de Ohm: I = V/R)
- Mostrar los valores calculados en panel de amperaje (solo si hay amperímetro)
- Simular el brillo de la bombilla según la intensidad con 4 niveles de color

**Interacción:**
- Botón para abrir/cerrar el interruptor (solo visible cuando hay interruptor en el circuito)
- Cambio de sprite del botón (imagen interruptor ON/OFF)
- Rotación del modelo 3D del interruptor al pulsarlo

**Generación procedural de cables:**
- Conectar visualmente los componentes del circuito con cables 3D en tiempo real
- Eliminar cables obsoletos antes de regenerarlos en cada ciclo
- Orientar y escalar cada cable según la posición real de los componentes en el espacio

#### Marcadores AR diseñados

Se diseñaron y registraron en Vuforia **20 image targets** repartidos en dos bases de datos:

| Base de datos | Marcadores | Descripción |
|---|---|---|
| `Practica_2_CDIG.xml` | 20 targets | Base principal con imágenes de los componentes (versión original + versión GPT mejorada) |
| `Practica2BD.xml` | 10 targets | Base de datos alternativa con segunda iteración de marcadores |

---

### Fase 2 — Diseño del Sistema

#### Arquitectura orientada a componentes

Unity impone un paradigma de **programación orientada a componentes** (diferente a la POO clásica): en lugar de jerarquías de clases, los objetos del juego (`GameObject`) son contenedores de **componentes** (`MonoBehaviour`, `Transform`, `Renderer`, `Button`...) que interactúan entre sí.

El diseño distribuyó las responsabilidades de la siguiente forma:

**`Control_Practica_2` (MonoBehaviour central):**
Componente script adjunto al GameObject `Controlador` en la escena. Centraliza toda la lógica de la aplicación y actúa como orquestador entre los diferentes subsistemas (detección AR, validación, simulación eléctrica, generación de cables, UI).

**ImageTargets de Vuforia:**
Cada marcador AR en la escena tiene configurado en su `DefaultTrackableEventHandler` los callbacks:
- `OnTrackingFound` → llama a `AgregarObjeto(GameObject)` del controlador
- `OnTrackingLost`  → llama a `EliminarObjeto(GameObject)` del controlador

Esto hace que el controlador mantenga siempre una lista actualizada de los objetos detectados en tiempo real.

**Prefabs 3D:**
Cada componente eléctrico se modeló como un **Prefab** Unity con estructura jerárquica de GameObjects, permitiendo instanciación en tiempo de ejecución y manipulación de sus partes (por ejemplo, acceder al hijo `"Top"` de `Bombilla3D` para cambiar su color).

**Canvas UI:**
Interfaz superpuesta sobre la imagen de la cámara con TextMesh Pro para el texto, imagen del interruptor y botón de interacción.

#### Decisiones de diseño clave

- **Timer en Update():** La validación no se ejecuta en cada frame (60fps) sino cada `intervaloOrdenacion` segundos (por defecto 3s), reduciendo la carga computacional y los artefactos visuales.
- **Ordenación por posición X:** Los componentes se ordenan por su coordenada X en el espacio 3D para determinar el orden del circuito, simulando la disposición física de izquierda a derecha.
- **Tuplas de retorno C#:** Los métodos `VerificarCircuito()` y `CalcularValoresElectricos()` usan **tuplas de valor** (`(bool, string)`, `(float, float, float)`) para devolver múltiples resultados sin crear clases auxiliares.
- **Constantes de string:** Todos los nombres de objetos y mensajes de estado se definen como `const string` al inicio de la clase, centralizando los "magic strings" y facilitando el mantenimiento.
- **Validación de Inspector en Start():** El método `VariablesInstanciadas()` verifica en el arranque que todas las referencias del Inspector están asignadas, evitando NullReferenceException en tiempo de ejecución.

---

### Fase 3 — Implementación en Unity y C#

#### Ciclo de vida Unity — Start() y Update()

La clase principal extiende `MonoBehaviour` y usa los dos métodos del ciclo de vida de Unity:

```csharp
public class Control_Practica_2 : MonoBehaviour
{
    void Start()
    {
        // 1. Validar que todas las variables del Inspector están asignadas
        instanciasCorrectas = VariablesInstanciadas(true);

        // 2. Inicializar listas y UI
        objetos3D = new List<GameObject>();
        verificarCircuitoTMP.text = CIRCUITO_SIN_ELEMENTOS;
        panelAmperaje.SetActive(false);
        botonInterruptor.gameObject.SetActive(false);

        // 3. Configurar listener del botón de interruptor
        botonInterruptor.onClick.AddListener(InterruptoPulsado);

        // 4. Estado inicial: bombilla apagada, interruptor abierto
        EncenderBombilla3D(0);
        CambiarInterruptor3D(estadoInterruptor);
    }

    void Update()
    {
        // Timer: solo ejecutar cada 'intervaloOrdenacion' segundos
        if (Time.time - ultimoTiempo < intervaloOrdenacion) return;
        ultimoTiempo = Time.time;

        // 1. Ordenar por posición X → determina orden del circuito
        objetos3D = objetos3D.OrderBy(o => o.transform.position.x).ToList();

        // 2. Validar circuito y mostrar resultado
        (bool valido, string motivo) = VerificarCircuito();

        // 3. Calcular valores eléctricos si el circuito es válido y el interruptor cerrado
        (float V, float R, float I) = (valido && estadoInterruptor)
            ? CalcularValoresElectricos()
            : (0f, 0f, 0f);

        // 4. Actualizar UI y bombilla
        EncenderBombilla3D(R);
        GenerarCables();
    }
}
```

#### Eventos AR — Registro dinámico de componentes

La integración con Vuforia funciona mediante el patrón **Observer**: cada ImageTarget llama al controlador cuando detecta o pierde su marcador:

```csharp
// Cuando Vuforia detecta el marcador → añade el componente al circuito
public void AgregarObjeto(GameObject nuevoObjeto)
{
    if (nuevoObjeto == null || objetos3D.Contains(nuevoObjeto)) return;
    objetos3D.Add(nuevoObjeto);
}

// Cuando Vuforia pierde el marcador → elimina el componente del circuito
public void EliminarObjeto(GameObject objetoAEliminar)
{
    if (objetoAEliminar != null && objetos3D.Contains(objetoAEliminar))
        objetos3D.Remove(objetoAEliminar);
}
```

Estos métodos están registrados directamente en la escena Unity como callbacks de los eventos `OnTrackingFound` y `OnTrackingLost` de cada ImageTarget, haciendo la integración completamente declarativa (sin código de Vuforia en el controlador).

#### Algoritmo de validación del circuito

El método `VerificarCircuito()` aplica 7 checks secuenciales, retornando en el primero que falla:

```csharp
private (bool esValido, string mensaje) VerificarCircuito()
{
    // 1. Lista vacía o nula
    if (objetos3D == null || objetos3D.Count == 0)
        return (false, ERROR_CIRCUITO_VACIO);

    // 2. Debe empezar con "Tierra Inicio" y terminar con "Tierra Fin"
    if (objetos3D[0].name != OBJETO_TIERRA_INICIO || objetos3D[^1].name != OBJETO_TIERRA_FIN)
        return (false, ERROR_TIERRA_INICIO_FIN);

    // 3. Mínimo 3 elementos (Tierra Inicio + algo + Tierra Fin)
    if (objetos3D.Count < 3)
        return (false, ERROR_CIRCUITO_CORTO);

    // 4. Segundo elemento debe ser la Fuente
    if (objetos3D[1].name != OBJETO_FUENTE)
        return (false, ERROR_SIN_FUENTE);

    // 5. Debe haber al menos una resistencia o bombilla
    // 6. No puede haber más de una resistencia
    int resistencias = 0;
    bool hayResistenciaOBombilla = false;
    for (int i = 2; i < objetos3D.Count - 1; i++)
    {
        if (EsResistencia(objetos3D[i].name)) { resistencias++; hayResistenciaOBombilla = true; }
        else if (objetos3D[i].name == OBJETO_BOMBILLA) hayResistenciaOBombilla = true;
    }

    if (!hayResistenciaOBombilla) return (false, ERROR_SIN_RESISTENCIA);
    if (resistencias > 1)        return (false, ERROR_MULTIPLES_RESISTENCIAS);

    // 7. Circuito válido
    return (true, CIRCUITO_VALIDO_MENSAJE);
}
```

#### Cálculo eléctrico — Ley de Ohm

```csharp
private (float voltaje, float resistencia, float intensidad) CalcularValoresElectricos(float voltajeFuente = 9f)
{
    // Activar panel de amperaje solo si hay amperímetro en el circuito
    panelAmperaje.SetActive(objetos3D.Any(o => o.name == OBJETO_AMPERIMETRO));

    // Acumular resistencias de cada componente detectado
    float resistenciaTotal = 0f;
    foreach (GameObject objeto in objetos3D)
    {
        switch (objeto.name)
        {
            case OBJETO_RESISTENCIA_1:    resistenciaTotal += 1f;    break;
            case OBJETO_RESISTENCIA_10:   resistenciaTotal += 10f;   break;
            case OBJETO_RESISTENCIA_100:  resistenciaTotal += 100f;  break;
            case OBJETO_RESISTENCIA_1000: resistenciaTotal += 1000f; break;
            case OBJETO_BOMBILLA:         resistenciaTotal += 1f;    break;
        }
    }

    // Ley de Ohm: I = V / R
    float intensidad = resistenciaTotal > 0 ? voltajeFuente / resistenciaTotal : 0f;

    return (voltajeFuente, resistenciaTotal, intensidad);
}
```

#### Generación procedural de cables 3D

Los cables se generan instanciando el prefab `Cilindro_Cable` y ajustando su posición, orientación y escala matemáticamente para conectar cada par de componentes consecutivos:

```csharp
private void ConectarCable(GameObject cable, Vector3 posIni, Vector3 posFin)
{
    Vector3 direccion = posFin - posIni;
    float distancia = direccion.magnitude;

    // Posicionar en el punto medio entre los dos componentes
    cable.transform.position = (posIni + posFin) / 2f;

    // Orientar el cilindro a lo largo de la dirección
    cable.transform.up = direccion.normalized;

    // Escalar: largo proporcional a la distancia, grosor constante
    cable.transform.localScale = new Vector3(GROSOR_CABLE * 2f, distancia / 2f, GROSOR_CABLE * 2f);
}

void GenerarCables()
{
    EliminarCablesPrevios(); // Destruir cables del ciclo anterior

    for (int i = 0; i < objetos3D.Count - 1; i++)
    {
        // Conectar desde el primer hijo de cada objeto al del siguiente
        Transform hijoIni = objetos3D[i].transform.GetChild(0);
        Transform hijoFin = objetos3D[i + 1].transform.GetChild(0);

        GameObject cable = Instantiate(cilindroCablePrefab, hijoIni.position, Quaternion.identity);
        cable.GetComponent<Renderer>().material.color = Color.black;
        ConectarCable(cable, hijoIni.position, hijoFin.position);
        cablesGenerados.Add(cable);
    }
}
```

#### Simulación visual de la bombilla

El color de la bombilla se adapta en tiempo real a la intensidad de corriente del circuito, usando 4 rangos de color que simulan el calentamiento del filamento:

```csharp
public void EncenderBombilla3D(float intensidad)
{
    Transform hijo = bombilla3D.transform.Find("Top"); // Parte luminosa del prefab
    Renderer renderer = hijo.GetComponent<Renderer>();

    renderer.material.color = intensidad switch
    {
        0f         => new Color(1f, 1f, 1f, 0f),      // Apagada (transparente)
        < 10f      => new Color(255, 140, 0),          // Baja    (naranja oscuro)
        < 100f     => new Color(255, 180, 50),         // Media   (amarillo anaranjado)
        < 1000f    => new Color(255, 220, 100),        // Alta    (amarillo claro)
        _          => new Color(255, 255, 200),        // Muy alta (blanco cálido)
    };
}
```

#### C# avanzado — Características del lenguaje utilizadas

| Característica | Uso en el proyecto |
|---|---|
| **Value Tuples** | `(bool, string)` y `(float, float, float)` como retorno múltiple |
| **LINQ** | `OrderBy()` para ordenar por posición, `Any()` para detectar presencia de componentes |
| **String interpolation** | `$"Objeto: {nombre} \| Pos X: {x}"` en todos los mensajes de debug |
| **const string** | Centralización de todos los nombres de objetos y mensajes de estado |
| **static readonly** | Colores predefinidos: `COLOR_VERDE`, `COLOR_ROJO` |
| **Pattern matching switch** | Selección de color por rango de intensidad |
| **Listeners en código** | `botonInterruptor.onClick.AddListener(InterruptoPulsado)` |
| **Generics** | `List<GameObject>` para objetos detectados y cables generados |

---

### Fase 4 — Pruebas y Validación

La naturaleza de una aplicación AR hace que las pruebas automáticas con JUnit no sean aplicables directamente. Las pruebas se realizaron mediante **validación funcional en dispositivo real**:

#### Pruebas de detección AR

| Caso de prueba | Descripción | Resultado esperado |
|---|---|---|
| Marcador único | Colocar solo un marcador ante la cámara | Objeto 3D aparece superpuesto |
| Marcador oculto | Tapar el marcador con la mano | Objeto 3D desaparece del circuito |
| Múltiples marcadores | Varios marcadores simultáneos | Todos los objetos 3D aparecen |
| Movimiento | Desplazar el marcador en el espacio | El objeto 3D sigue al marcador |

#### Pruebas de validación del circuito

| Configuración | Error esperado |
|---|---|
| Sin elementos | `"El circuito está vacío o no inicializado"` |
| Solo Tierra Inicio | `"El circuito no comienza ni termina correctamente"` |
| Tierra + Fuente sin resistencia | `"Debe haber al menos una resistencia o bombilla"` |
| Dos resistencias | `"No pueden haber dos o más resistencias"` |
| Tierra + Fuente + Resistencia + Tierra | ✅ `"Circuito correcto"` |

#### Pruebas de cálculo eléctrico

| Componentes | V (Voltios) | R (Ohmios) | I esperada (A) |
|---|---|---|---|
| Resistencia 1Ω | 9 | 1 | 9.000 |
| Resistencia 10Ω | 9 | 10 | 0.900 |
| Resistencia 100Ω | 9 | 100 | 0.090 |
| Resistencia 1000Ω | 9 | 1000 | 0.009 |
| Bombilla (1Ω) | 9 | 1 | 9.000 |

#### Pruebas de la bombilla

| Intensidad | Color esperado |
|---|---|
| 0 (apagada / interruptor abierto) | Transparente |
| < 10 A | Naranja oscuro |
| 10 – 99 A | Amarillo anaranjado |
| 100 – 999 A | Amarillo claro |
| ≥ 1000 A | Blanco cálido |

---

## Arquitectura del Sistema

```
┌─────────────────────────────────────────────────────────────────┐
│                       UNITY SCENE                               │
│                                                                 │
│  ┌──────────────┐   ┌───────────────────────────────────────┐  │
│  │   ARCamera   │   │              CANVAS (UI)              │  │
│  │  (Vuforia)   │   │  ┌──────────────┐ ┌───────────────┐  │  │
│  │              │   │  │ Panel Circuito│ │ Panel Amperaje│  │  │
│  │  Detecta     │   │  │ TMP_Text     │ │ TMP_Text      │  │  │
│  │  marcadores  │   │  │ (verde/rojo) │ │ (V, R, I)     │  │  │
│  └──────┬───────┘   │  └──────────────┘ └───────────────┘  │  │
│         │           │  ┌──────────────────────────────────┐  │  │
│         │ detecta   │  │ Botón Interruptor (ON/OFF sprite)│  │  │
│         ▼           │  └──────────────────────────────────┘  │  │
│  ┌──────────────────────────────────────────────────────────┐ │  │
│  │                    ACTIVADORES                           │ │  │
│  │   ImageTarget (Bombilla) ──OnFound──► AgregarObjeto()   │ │  │
│  │   ImageTarget (Fuente)   ──OnLost───► EliminarObjeto()  │ │  │
│  │   ImageTarget (Resistencia 1)  ...                      │ │  │
│  │   ImageTarget (Resistencia 10) ...   (x10 targets)      │ │  │
│  └──────────────────────┬───────────────────────────────────┘ │  │
│                         │ llaman a                            │  │
│  ┌──────────────────────▼───────────────────────────────────┐ │  │
│  │              CONTROLADOR (Control_Practica_2)            │ │  │
│  │                                                          │ │  │
│  │  List<GameObject> objetos3D  ←── registro dinámico       │ │  │
│  │                                                          │ │  │
│  │  Update() cada 3s:                                       │ │  │
│  │  1. OrderBy(position.X)    → orden del circuito          │ │  │
│  │  2. VerificarCircuito()    → (bool, string)              │ │  │
│  │  3. CalcularElectricos()   → (V, R, I) Ley de Ohm        │ │  │
│  │  4. EncenderBombilla3D(R)  → color por intensidad        │ │  │
│  │  5. GenerarCables()        → Instantiate + ConectarCable │ │  │
│  └──────────────────────────────────────────────────────────┘ │  │
│                                                                 │
│  Directional Light  ·  EventSystem                             │
└─────────────────────────────────────────────────────────────────┘
```

---

## Componentes del Circuito

### Marcadores físicos (Image Targets)

| Componente | Nombre en escena | Valor resistivo | Rol en el circuito |
|---|---|---|---|
| 🌍 Tierra Inicio | `Tierra Inicio` | — | Inicio obligatorio del circuito |
| 🌍 Tierra Final | `Tierra Fin` | — | Final obligatorio del circuito |
| ⚡ Fuente 5V | `Fuente` | — | Fuente de alimentación (9V en simulación) |
| 💡 Bombilla | `Bombilla` | 1 Ω | Carga resistiva + indicador visual de corriente |
| 🔀 Interruptor | `Interruptor` | — | Abre/cierra el circuito (control por botón UI) |
| 📊 Amperímetro | `Amperimetro` | — | Activa el panel de lectura de valores eléctricos |
| ⬛ Resistencia 1Ω | `Resistencia 1` | 1 Ω | Resistencia mínima |
| ⬛ Resistencia 10Ω | `Resistencia 10` | 10 Ω | Resistencia baja |
| ⬛ Resistencia 100Ω | `Resistencia 100` | 100 Ω | Resistencia media |
| ⬛ Resistencia 1000Ω | `Resistencia 1000` | 1000 Ω | Resistencia alta |

### Prefabs 3D

| Prefab | Estructura interna | Descripción |
|---|---|---|
| `Bombilla3D` | `Top` + `Bottom` + `bulb-inside` | El hijo `Top` cambia de color según la corriente |
| `Pila3D` | `Cabeza`, `Medio`, `Anillos`, `Toroide_1/2/3` | Modelo de pila cilíndrica con anillos |
| `Amperimetro3D` | `default` | Modelo simple de amperímetro |
| `Interruptor3D` | `default` | Se rota 180° en Z al pulsar el botón |
| `Switch3D` | `10121_Light_Switch_v1` | Interruptor de pared con modelo externo |
| `Resistencia3D` | `default` | Resistencia genérica |
| `Tierra3D` | `16545_PlanetSigns_Earth` | Símbolo de tierra con modelo de planeta |
| `Cilindro_Cable` | Cilindro primitivo | Se instancia, orienta y escala proceduralmente |
| `BombillaAdrian3D` | Modelo alternativo | Segunda versión de la bombilla |

---

## Algoritmos y Lógica de Negocio

### Algoritmo de validación — 7 checks secuenciales

```
Entrada: List<GameObject> objetos3D (ordenados por posición X)

Check 1: ¿Lista vacía o nula?
         → Error: "El circuito está vacío o no inicializado"

Check 2: ¿objetos3D[0] == "Tierra Inicio"
          AND objetos3D[último] == "Tierra Fin"?
         → Error: "El circuito no comienza ni termina correctamente con Tierra"

Check 3: ¿Count < 3?
         → Error: "El circuito es demasiado corto para ser válido"

Check 4: ¿objetos3D[1] == "Fuente"?
         → Error: "Después de Tierra Inicio debería estar la Fuente"

Check 5: ¿Hay al menos una Resistencia o Bombilla en posiciones 2..n-1?
         → Error: "Debe haber al menos una resistencia o bombilla en el circuito"

Check 6: ¿Hay más de una Resistencia?
         → Error: "No pueden haber dos o más resistencias en el circuito"

Check 7: ✅ Todos los checks superados → Circuito correcto
```

### Cálculo eléctrico — Ley de Ohm

```
Voltaje fuente: V = 9V (constante)

Resistencia total: R = Σ(resistencia de cada componente detectado)
  Resistencia 1    → +1 Ω
  Resistencia 10   → +10 Ω
  Resistencia 100  → +100 Ω
  Resistencia 1000 → +1000 Ω
  Bombilla         → +1 Ω

Intensidad: I = V / R  (Ley de Ohm)
            I = 0 si R = 0 (prevención de división por cero)

Panel amperaje: solo visible si "Amperímetro" está en la lista
```

### Generación de cables — Geometría 3D

```
Para cada par consecutivo (i, i+1) en objetos3D:
  posIni = primer hijo de objetos3D[i].transform.position
  posFin = primer hijo de objetos3D[i+1].transform.position

  direccion = posFin - posIni
  distancia = |direccion|

  cable.position  = (posIni + posFin) / 2     → punto medio
  cable.up        = direccion.normalized        → orientación
  cable.scale     = (0.2, distancia/2, 0.2)    → grosor × longitud
```

---

## Estructura del Proyecto

```
AR/
└── Assets/
    ├── Scripts/
    │   └── Control_Practica_2.cs          # Script principal (MonoBehaviour)
    ├── script.cs                          # Script auxiliar (rotación/traslación)
    ├── Prefabs/
    │   ├── Bombilla3D.prefab              # Bombilla con hijo "Top" para color
    │   ├── BombillaAdrian3D.prefab        # Bombilla alternativa
    │   ├── Pila3D.prefab                  # Pila con toroides
    │   ├── Amperimetro3D.prefab           # Amperímetro
    │   ├── Interruptor3D.prefab           # Interruptor (se rota al pulsar)
    │   ├── Resistencia3D.prefab           # Resistencia genérica
    │   ├── Switch3D.prefab                # Switch tipo pared
    │   ├── Tierra3D.prefab                # Símbolo de tierra
    │   └── Cilindro_Cable.prefab          # Cable (instanciación procedural)
    ├── Scenes/
    │   └── Escena Practica 2.unity        # Escena principal con todos los GameObjects
    ├── Imagenes/
    │   ├── Interruptor_ON.jpg             # Sprite interruptor cerrado
    │   └── Interruptor_OFF.jpg            # Sprite interruptor abierto
    ├── StreamingAssets/Vuforia/
    │   ├── Practica_2_CDIG.xml            # Base de datos principal (20 targets)
    │   └── Practica2BD.xml                # Base de datos alternativa (10 targets)
    ├── Editor/Vuforia/
    │   ├── ImageTargetTextures/
    │   │   ├── Practica_2_CDIG/           # Texturas de los 20 marcadores
    │   │   └── Practica2BD/               # Texturas alternativas
    │   └── ForPrint/                      # PDFs imprimibles de marcadores
    ├── Resources/
    │   └── VuforiaConfiguration.asset     # Configuración y licencia de Vuforia
    └── TextMesh Pro/                      # Plugin TMP para texto de alta calidad
```

---

## Tecnologías y Herramientas

| Tecnología | Versión | Uso |
|---|---|---|
| **Unity** | 2022.x | Motor de juego y runtime AR |
| **C#** | 9+ | Scripting, lógica de negocio y simulación |
| **Vuforia Engine** | 10.x | SDK de reconocimiento de marcadores AR |
| **TextMesh Pro** | — | Texto de alta calidad en la UI |
| **Android Build Tools** | — | Compilación y despliegue en dispositivos Android |
| **Git** | — | Control de versiones |

### Componentes Unity utilizados

| Componente | Propósito |
|---|---|
| `MonoBehaviour` | Clase base del script principal |
| `GameObject` | Contenedor de todos los objetos 3D |
| `Transform` | Posición, rotación y escala en el espacio 3D |
| `Renderer` / `MeshRenderer` | Renderizado y cambio de color de los objetos |
| `Button` | Interacción con el interruptor |
| `Image` | Cambio de sprite ON/OFF del interruptor |
| `TMP_Text` | Visualización de estado y valores eléctricos |
| `List<GameObject>` | Colección dinámica de componentes detectados |
| `Instantiate` / `Destroy` | Generación y destrucción procedural de cables |
| `Time.time` | Timer para controlar el intervalo de actualización |

---

## Instalación y Configuración

### Prerrequisitos

- Unity 2022.x con módulo **Android Build Support**
- **Vuforia Engine** instalado (Unity Package Manager → Add by URL)
- Dispositivo Android con cámara (API 26+)
- Licencia de Vuforia (versión gratuita disponible con marca de agua)
- Marcadores impresos (PDFs en `Assets/Editor/Vuforia/ForPrint/`)

### Pasos de configuración

1. Abrir la carpeta `AR/` con Unity Hub
2. En `Assets/Resources/VuforiaConfiguration.asset`, introducir la **licencia de Vuforia**
3. Abrir la escena `Assets/Scenes/Escena Practica 2.unity`
4. Verificar que el componente `Control_Practica_2` en el Inspector tiene todas las referencias asignadas
5. Compilar para Android: `File → Build Settings → Android → Build And Run`
6. Imprimir los marcadores del PDF en `Assets/Editor/Vuforia/ForPrint/ImageTargets/`

### Variables del Inspector (obligatorias)

| Variable | Tipo | Descripción |
|---|---|---|
| `objetos3D` | `List<GameObject>` | Lista de GameObjects detectables |
| `verificarCircuitoTMP` | `TMP_Text` | Texto de estado del circuito |
| `amperajeTMP` | `TMP_Text` | Texto de valores V, R, I |
| `bombilla3D` | `GameObject` | Prefab de la bombilla (para cambiar color) |
| `interruptor3D` | `GameObject` | Prefab del interruptor (para rotarlo) |
| `cilindroCablePrefab` | `GameObject` | Prefab del cable (para instanciar) |
| `botonInterruptor` | `Button` | Botón UI del interruptor |
| `imgInterruptor` | `Image` | Imagen del botón (ON/OFF) |
| `spriteInterruptorOFF` | `Sprite` | Sprite del interruptor abierto |
| `spriteInterruptorON` | `Sprite` | Sprite del interruptor cerrado |
| `panelAmperaje` | `GameObject` | Panel de valores eléctricos |

---

## Uso de la Aplicación

1. **Abrir la app** en el dispositivo Android
2. **Apuntar la cámara** a las tarjetas impresas sobre una superficie plana
3. Los **objetos 3D** aparecen superpuestos sobre cada tarjeta reconocida
4. Cada **N segundos** (configurable), el sistema valida el circuito automáticamente
5. El **texto de estado** cambia a verde (circuito correcto) o rojo (error con descripción)
6. Si hay interruptor en el circuito, aparece el **botón ON/OFF** en pantalla
7. Al **cerrar el interruptor**, se calcula la corriente y se muestran V, R e I en el panel
8. La **bombilla 3D** cambia de color según la intensidad: naranja → amarillo → blanco
9. Los **cables 3D** se regeneran conectando los componentes en el orden del circuito

## Authors and acknowledgment
Show your appreciation to those who have contributed to the project.

## License
For open source projects, say how it is licensed.

## Project status
If you have run out of energy or time for your project, put a note at the top of the README saying that development has slowed down or stopped completely. Someone may choose to fork your project or volunteer to step in as a maintainer or owner, allowing your project to keep going. You can also make an explicit request for maintainers.
