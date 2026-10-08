using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using System.Linq; 

public class Control_Practica_2 : MonoBehaviour
{

    private const string ERROR_INSTANCIAS = "Variable publica no instanciada, se abortara.";
    private const string AMPERAJE_SIN_MEDICION = "Amperaje: Sin medicion";
    private const string CIRCUITO_SIN_ELEMENTOS = "Circuito sin elementos";
    private const string CIRCUITO_CORRECTO = "Circuito correcto";
    private const string CIRCUITO_CON_ERRORES = "Circuito con errores";
    private const string ERROR_CIRCUITO_VACIO = "El circuito esta vacio o no inicializado.";
    private const string ERROR_TIERRA_INICIO_FIN = "El circuito no comienza ni termina correctamente con Tierra.";
    private const string ERROR_CIRCUITO_CORTO = "El circuito es demasiado corto para ser valido.";
    private const string ERROR_SIN_FUENTE = "Despues de Tierra Inicio deberia estar la Fuente.";
    private const string ERROR_SIN_RESISTENCIA = "Debe haber al menos una resistencia o bombilla en el circuito.";
    private const string ERROR_MULTIPLES_RESISTENCIAS = "No pueden haber dos o mas resistencias en el circuito.";
    private const string CIRCUITO_VALIDO_MENSAJE = "El circuito cumple con todas las condiciones.";
    private const string OBJETO_TIERRA_INICIO = "Tierra Inicio";
    private const string OBJETO_TIERRA_FIN = "Tierra Fin";
    private const string OBJETO_FUENTE = "Fuente";
    private const string OBJETO_BOMBILLA = "Bombilla";
    private const string OBJETO_INTERRUPTOR = "Interruptor";
    private const string OBJETO_AMPERIMETRO = "Amperimetro";
    private const string OBJETO_RESISTENCIA_1 = "Resistencia 1";
    private const string OBJETO_RESISTENCIA_10 = "Resistencia 10";
    private const string OBJETO_RESISTENCIA_100 = "Resistencia 100";
    private const string OBJETO_RESISTENCIA_1000 = "Resistencia 1000";
    private const float GROSOR_CABLE = 0.10f; 
    private static readonly Color COLOR_VERDE = new Color(0f, 0.5f, 0f);  
    private static readonly Color COLOR_ROJO = new Color(0.5f, 0f, 0f);  

    public List<GameObject> objetos3D; 
    public float intervaloOrdenacion = 3f; //tiempo 
    public TMP_Text verificarCircuitoTMP; 
    public TMP_Text amperajeTMP; 
    public GameObject bombilla3D; 
    public GameObject interruptor3D; 
    public GameObject cilindroCablePrefab; 
    public Button botonInterruptor; 
    public Image imgInterruptor; 
    public Sprite spriteInterruptorOFF; //abierto
    public Sprite spriteInterruptorON; //cerrado
    public GameObject panelAmperaje; 

    private bool estadoInterruptor = false; //apagado
    private bool instanciasCorrectas = false;
    private float ultimoTiempo = 0f;

    private List<GameObject> cablesGenerados = new List<GameObject>(); //cables instanciados

    void Start()
    {

        instanciasCorrectas = VariablesInstanciadas(true);
        if (!instanciasCorrectas)
        {
            verificarCircuitoTMP.text = ERROR_INSTANCIAS;
            return; 
        }

        objetos3D = new List<GameObject>(); 
        ultimoTiempo = Time.time; 

        verificarCircuitoTMP.text = CIRCUITO_SIN_ELEMENTOS;
        verificarCircuitoTMP.color = Color.black;
        amperajeTMP.text = AMPERAJE_SIN_MEDICION;
        amperajeTMP.color = Color.black;
        panelAmperaje.SetActive(false); 
        botonInterruptor.gameObject.SetActive(false); 

        EncenderBombilla3D(0); 

        botonInterruptor.onClick.AddListener(InterruptoPulsado); 
        imgInterruptor.sprite = spriteInterruptorOFF; 
        CambiarInterruptor3D(estadoInterruptor);

    }

    void Update()
    {

        if (Time.time - ultimoTiempo < intervaloOrdenacion)
        {
            return; 
        }

        ultimoTiempo = Time.time; 
        
        if (!instanciasCorrectas) {
            return; 
        }

        if (objetos3D == null || objetos3D.Count == 0)
        {
            Debug.LogWarning("Circuito sin elementos.");
            verificarCircuitoTMP.text = CIRCUITO_SIN_ELEMENTOS;
            verificarCircuitoTMP.color = Color.black;
            amperajeTMP.text = AMPERAJE_SIN_MEDICION;
            amperajeTMP.color = Color.black;
            panelAmperaje.SetActive(false); 
            botonInterruptor.gameObject.SetActive(false);
            return;
        }

        if (TieneInterruptor())
            botonInterruptor.gameObject.SetActive(true); 
        else
            botonInterruptor.gameObject.SetActive(false); 

        objetos3D = objetos3D.OrderBy(objeto => objeto.transform.position.x).ToList();

        Debug.LogWarning("Lista de objetos ordenada por posicion en X:");
        foreach (GameObject objeto in objetos3D)
        {
            Debug.LogWarning($"Objeto: {objeto.name} | Posicion X: {objeto.transform.position.x}");
        }

        (bool circuitoValido, string motivo) = VerificarCircuito();

        Debug.LogWarning($"Resultado de la verificacion del circuito: {circuitoValido} | Motivo: {motivo}");

        verificarCircuitoTMP.text = circuitoValido
            ? $"{CIRCUITO_CORRECTO}\n{motivo}"
            : $"{CIRCUITO_CON_ERRORES}\n{motivo}";

        verificarCircuitoTMP.color = circuitoValido ? COLOR_VERDE : COLOR_ROJO;

        (float voltaje, float resistencia, float intensidad) =
            (circuitoValido && estadoInterruptor) ? CalcularValoresElectricos() : (0f, 0f, 0f);

        string intensidadTexto = intensidad.ToString("F3");

        Debug.Log($" Voltaje: {voltaje} V | Resistencia: {resistencia} Ω | Intensidad: {intensidadTexto} A");

        amperajeTMP.text = $"Voltaje: {voltaje}V\nResistencia: {resistencia}Ω\nIntensidad: {intensidadTexto}A";

        amperajeTMP.color = (intensidad == 0f) ? Color.black : Color.blue;

        EncenderBombilla3D(resistencia);

        GenerarCables();
    }

    public void AgregarObjeto(GameObject nuevoObjeto)
    {
        if (nuevoObjeto == null)
        {
            Debug.LogError(" No se puede agregar un objeto null");
            return;
        }

        if (objetos3D.Contains(nuevoObjeto))
        {
            Debug.LogWarning($" El objeto `{nuevoObjeto.name}` ya esta en la lista.");
            return;
        }

        objetos3D.Add(nuevoObjeto);
        Debug.Log($" Objeto `{nuevoObjeto.name}` agregado correctamente.");
    }

    public void EliminarObjeto(GameObject objetoAEliminar)
    {
        if (objetoAEliminar == null)
        {
            Debug.LogError(" No se puede eliminar un objeto `null`");
            return;
        }

        if (objetos3D.Contains(objetoAEliminar))
        {
            objetos3D.Remove(objetoAEliminar); 
            Debug.Log($" Objeto `{objetoAEliminar.name}` eliminado correctamente.");
        }
        else
        {
            Debug.LogWarning($" El objeto `{objetoAEliminar.name}` no esta en la lista.");
        }
    }

    private (bool esValido, string mensaje) VerificarCircuito()
    {
        if (objetos3D == null || objetos3D.Count == 0)
        {
            return (false, ERROR_CIRCUITO_VACIO);
        }

        if (objetos3D[0].name != OBJETO_TIERRA_INICIO || objetos3D[objetos3D.Count - 1].name != OBJETO_TIERRA_FIN)
        {
            return (false, ERROR_TIERRA_INICIO_FIN);
        }

        if (objetos3D.Count < 3)
        {
            return (false, ERROR_CIRCUITO_CORTO);
        }

        if (objetos3D[1].name != OBJETO_FUENTE)
        {
            return (false, ERROR_SIN_FUENTE);
        }

        bool hayResistenciaOBombilla = false;
        bool tieneMultiplesResistencias = false;
        int resistenciasEncontradas = 0;

        for (int i = 2; i < objetos3D.Count - 1; i++)
        {
            string nombreObjeto = objetos3D[i].name;

            if (nombreObjeto == OBJETO_RESISTENCIA_1 || nombreObjeto == OBJETO_RESISTENCIA_10 ||
                nombreObjeto == OBJETO_RESISTENCIA_100 || nombreObjeto == OBJETO_RESISTENCIA_1000)
            {
                resistenciasEncontradas++;
                hayResistenciaOBombilla = true;

                if (resistenciasEncontradas > 1)
                {
                    tieneMultiplesResistencias = true;
                }
            }
            else if (nombreObjeto == OBJETO_BOMBILLA)
            {
                hayResistenciaOBombilla = true;
            }
        }

        if (!hayResistenciaOBombilla)
        {
            return (false, ERROR_SIN_RESISTENCIA);
        }

        if (tieneMultiplesResistencias)
        {
            return (false, ERROR_MULTIPLES_RESISTENCIAS);
        }

        return (true, CIRCUITO_VALIDO_MENSAJE);
    }


    private (float voltaje, float resistencia, float intensidad) CalcularValoresElectricos(float voltajeFuente = 9f)
    {
        if (objetos3D == null || objetos3D.Count == 0)
        {
            Debug.LogWarning(" Circuito sin elementos.");
            return (voltajeFuente, 0f, 0f);
        }

        panelAmperaje.SetActive(objetos3D.Any(o => o.name == OBJETO_AMPERIMETRO));

        float resistenciaTotal = 0f;
        foreach (GameObject objeto in objetos3D)
        {
            switch (objeto.name)
            {
                case OBJETO_RESISTENCIA_1: resistenciaTotal += 1f; break;
                case OBJETO_RESISTENCIA_10: resistenciaTotal += 10f; break;
                case OBJETO_RESISTENCIA_100: resistenciaTotal += 100f; break;
                case OBJETO_RESISTENCIA_1000: resistenciaTotal += 1000f; break;
                case OBJETO_BOMBILLA: resistenciaTotal += 1f; break;
            }

            Debug.LogWarning($" Objeto: {objeto.name} | Resistencia acumulada: {resistenciaTotal} Ω");
        }

        // Ley de Ohm
        float intensidad = resistenciaTotal > 0 ? voltajeFuente / resistenciaTotal : 0f;

        Debug.Log($" Intensidad: {intensidad} A | Voltaje: {voltajeFuente} V | Resistencia total: {resistenciaTotal} Ω");

        return (voltajeFuente, resistenciaTotal, intensidad);
    }

    
private void ConectarCable(GameObject cable, Vector3 posIni, Vector3 posFin)
{
    Vector3 direccion = posFin - posIni;
    float distancia = direccion.magnitude;

    // Posicionar el cilindro en el punto medio
    cable.transform.position = (posIni + posFin) / 2f;

    // Orientar el cilindro a lo largo de la direccion Y
    cable.transform.up = direccion.normalized;

    //Ajustar escala: largo en Y, grosor en X/Z
    cable.transform.localScale = new Vector3(GROSOR_CABLE * 2f, distancia / 2f, GROSOR_CABLE * 2f);
}

void GenerarCables()
{
    EliminarCablesPrevios(); 

    if (objetos3D == null || objetos3D.Count < 2)
    {
        Debug.LogWarning("No hay suficientes elementos para generar cables.");
        return;
    }

    for (int i = 0; i < objetos3D.Count - 1; i++)
    {
        //primer hijo de cada objeto
        Transform hijoIni = objetos3D[i].transform.childCount > 0 ? objetos3D[i].transform.GetChild(0) : null;
        Transform hijoFin = objetos3D[i + 1].transform.childCount > 0 ? objetos3D[i + 1].transform.GetChild(0) : null;

        if (hijoIni == null || hijoFin == null)
        {
            Debug.LogWarning($" Saltando conexion entre {objetos3D[i].name} y {objetos3D[i + 1].name} porque uno no tiene hijo.");
            continue;
        }

        Vector3 posIni = hijoIni.position;
        Vector3 posFin = hijoFin.position;

        
        GameObject nuevoCable = Instantiate(cilindroCablePrefab, posIni, Quaternion.identity);
        nuevoCable.GetComponent<Renderer>().material.color = Color.black;

        //orientar el cable desde posIni a posFin (centros reales)
        ConectarCable(nuevoCable, posIni, posFin);

        cablesGenerados.Add(nuevoCable);
    }
}

    void EliminarCablesPrevios()
    {
        foreach (GameObject cable in cablesGenerados)
        {
            Destroy(cable); 
        }
        cablesGenerados.Clear(); //Limpia la lista 
    }

    void InterruptoPulsado()
    {
        estadoInterruptor = !estadoInterruptor; // Alternar estado
        imgInterruptor.sprite = estadoInterruptor ? spriteInterruptorON : spriteInterruptorOFF; // Cambiar imagen

        CambiarInterruptor3D(estadoInterruptor);

        ultimoTiempo = Time.time - intervaloOrdenacion; //Forzar ejecucion en el Update()
    }

    private bool TieneInterruptor()
    {
        return objetos3D.Any(o => o.name == OBJETO_INTERRUPTOR);
    }

    private void CambiarInterruptor3D(bool encendido)
    {
        if (encendido==true){
            interruptor3D.transform.Rotate(0f,0f,180f);
        }
        else{
            interruptor3D.transform.Rotate(0f,0f,180f);
        }
        
    }

    public void EncenderBombilla3D(float intensidad)
    {
        // Buscar el hijo "Top" dentro de bombilla3D
        Transform hijo = bombilla3D.transform.Find("Top");

        if (hijo != null)
        {
            Renderer renderer = hijo.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Definir el material segun la intensidad
                if (intensidad == 0)
                {
                    renderer.material.color = new Color(1f, 1f, 1f, 0f); //Bombilla transparente
                }
                else if (intensidad < 10)
                {
                    renderer.material.color = new Color(255, 140, 0); // Intensidad Baja (Naranja oscuro)
                }
                else if (intensidad < 100)
                {
                    renderer.material.color = new Color(255, 180, 50); //Intensidad Media (Amarillo anaranjado)
                }
                else if (intensidad < 1000)
                {
                    renderer.material.color = new Color(255, 220, 100); //Intensidad Alta (Amarillo claro)
                }
                else
                {
                    renderer.material.color = new Color(255, 255, 200); //Intensidad Muy Alta (Blanco calido)
                }
            }
            else
            {
                Debug.LogWarning("No se encontro el componente Renderer en el hijo.");
            }
        }
        else
        {
            Debug.LogWarning("No se encontro el hijo 'Top'.");
        }
    }

    private bool VariablesInstanciadas(bool mostrarDebug)
    {
        bool todasInstanciadas = true;

        //Lista variables a verificar
        object[] variables = {
        verificarCircuitoTMP, amperajeTMP, bombilla3D, cilindroCablePrefab,
        botonInterruptor, imgInterruptor, spriteInterruptorOFF, spriteInterruptorON,
        panelAmperaje, interruptor3D
    };

        string[] nombres = {
        "verificarCircuitoTMP", "amperajeTMP", "bombilla3D", "cilindroCablePrefab",
        "botonInterruptor", "imgInterruptor", "spriteInterruptorOFF", "spriteInterruptorON",
        "panelAmperaje", "interruptor3D"
    };

        //Recorrer todas las variables y verificar si estan instanciadas
        for (int i = 0; i < variables.Length; i++)
        {
            if (variables[i] == null)
            {
                todasInstanciadas = false;
                if (mostrarDebug) Debug.LogError($"ERROR: La variable '{nombres[i]}' es NULL.");
            }
        }

        if (todasInstanciadas && mostrarDebug)
        {
            Debug.Log("Todas las variables estan correctamente instanciadas.");
        }

        return todasInstanciadas;
    }

}
