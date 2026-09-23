# PixelVault 🎮

PixelVault è un'applicazione web gestionale sviluppata in **.NET Core Razor Pages** e **MongoDB**, progettata per la gestione completa dell'inventario e del catalogo di videogiochi, editori, sviluppatori e vendite multi-piattaforma.

---

## 🚀 Caratteristiche Principali

* **Architettura Multi-Genere:** Gestione flessibile tramite tag/generi multipli (`List<string>`) per singolo videogioco.
* **Ricerca e Filtraggio Avanzato:** Filtra il catalogo dinamico per titolo, generi multipli, sviluppatore ed editore.
* **Calcolo Dinamico dei Prezzi:** Gestione automatica di sconti percentuale e calcolo in tempo reale del prezzo finale.
* **Persistenza NoSQL con MongoDB:** Integrazione con driver nativo MongoDB e indicizzazione ottimizzata su collezioni chiave.
* **Interfaccia Responsive & UI/UX Cura:** Dashboard pulita e moderna sviluppata con Bootstrap 5, layout a tabelle ottimizzato per prevenire overflow e navigazione fluida.
* **Data Validation & Localizzazione:** Validazione rigorosa lato client e server (rating 1-10, sconti 0-100%, date).

---

## 🛠️ Tech Stack

* **Framework:** .NET Core (Razor Pages)
* **Database:** MongoDB (istanziato tramite Docker)
* **ORM / Driver:** MongoDB C# Official Driver
* **Frontend:** Razor HTML5, CSS3, Bootstrap 5

---

## 🗄️ Struttura del Database

Il database NoSQL comprende le seguenti collezioni principali:

* **`Games`**: Contiene titolo, generi (`GenreIds`), sviluppatore, editore, prezzo, sconto, valutazione e data di uscita.
* **`Genres`**: Catalogo dei generi disponibili.
* **`Developers`**: Anagrafica degli sviluppatori.
* **`Publishers`**: Anagrafica degli editori/publisher.
* **`Inventories`**: Gestione scorte e disponibilità divisa per piattaforma.
* **`Sales`**: Storico e registrazione delle vendite effettuate.