# Moto Terrores

## Resumen

Te quedas sin nafta en el medio de la ruta. Al bajar a revisar un galpón cercano para buscar un bidón de nafta o algo que te ayude, te encontras con **El Mikilo**. La criatura comenzará a perseguirte implacablemente para matarte. Tu único objetivo es encontrar la nafta, volver al auto y escapar de ahí con vida.

## Características Principales

- **Supervivencia y Escape:** Explora el oscuro entorno, encuentra el combustible y llega a tu vehículo para ganar el juego.
- **Inteligencia Artificial (El Mikilo):** Un enemigo controlado por un sistema de IA que patrulla la zona y te persigue automáticamente en cuanto entras en su rango de visión.
- **Salud:** SI te encuentre te mata y es _Game Over_.
- **Perspectiva:** Juego en tercera persona con controles optimizados.

## Estructura Técnica del Proyecto (Unity)

El proyecto está construido en Unity y cuenta con los siguientes sistemas principales:

- `AIController.cs`: Maneja los estados de patrullaje y persecución del enemigo.
- `HealthSystem.cs`: Gestiona la cantidad de vidas del jugador y transiciona a la pantalla de Game Over.
- `ObjetivoNivel.cs`: Define el trigger final (tu auto/estación) que, al ser alcanzado, activa la secuencia de victoria y escape.
