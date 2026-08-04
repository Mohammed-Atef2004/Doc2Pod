
# 🎙️ Doc2Pod: Multimodal AI Educational Podcast Generator

Doc2Pod is an end-to-end, AI-powered educational platform that transforms complex academic documents (PDFs, slides, charts, and code snippets) into engaging, multi-speaker conversational podcasts. The generated podcasts utilize code-switching (Egyptian Arabic and Technical English) to make dense Computer Science topics accessible and entertaining.

This repository is structured as a **Monorepo**, housing three decoupled microservices: a web-based Frontend, an Enterprise-grade Backend, and a GPU-accelerated AI Inference Engine.
---

## 🚀Demo
https://github.com/user-attachments/assets/3e70a78e-4c64-4a9b-815d-bbce2fd9fcb1

## 🚀 System Architecture & Microservices

### 1. 🖥️ Frontend (Angular SPA)
* **Path:** `/FrontEnd`
* **Tech Stack:** Angular, TypeScript, HTML/SCSS, Bootstrap
* **Description:** A highly responsive Single Page Application (SPA) providing role-based dashboards for Students and Instructors. Users can upload course materials, manage episodes, and play generated podcasts directly in the browser.

### 2. ⚙️ Backend (ASP.NET Core API)
* **Path:** `/Backend`
* **Tech Stack:** C#, ASP.NET Core, Entity Framework Core (SQL Server), JWT
* **Architecture:** Built using **Clean Architecture** and **CQRS** (via MediatR) alongside Domain-Driven Design (DDD) principles.
* **Description:** Handles secure user authentication, SMTP email onboarding, role-based access control, and the core relational database logic for managing Courses, Users, and Episodes.

### 3. 🧠 AI Services & Inference Engine (FastAPI & Colab)
* **Path:** `/ExternalServices` (Jupyter Notebooks & Python Scripts)
* **Tech Stack:** Python, FastAPI, PyTorch, Hugging Face, Supabase, Cloudflare Tunnels
* **Description:** The heavy-lifting GPU pipeline. It exposes a thread-safe FastAPI endpoint via Cloudflare Tunnels to communicate with the C# backend. 
  * **Visual Understanding:** Uses **PaddleOCR** and **Qwen3-VL** to extract text, tables, and describe complex diagrams.
  * **Semantic RAG:** Chunks the data and embeds it into **ChromaDB** using `BAAI/bge-m3`.
  * **Script Generation:** Utilizes a fine-tuned **Qwen LLM** with state-aware prompt engineering to generate natural, code-switched podcast dialogue (Sara & Ahmed).
  * **Text-to-Speech (TTS):** Employs a custom-tuned **VibeVoice** diffusion model to generate ultra-realistic Egyptian voices, compiling the final multi-speaker track via **FFmpeg** and syncing it to **Supabase** cloud storage.

---

## ✨ Key Engineering Achievements
* **Memory-Optimized GPU Pipeline:** Engineered an aggressive garbage collection and lazy-loading architecture in Python to sequentially load heavy Vision and LLM models on a single T4 GPU without Out-Of-Memory (OOM) crashes.
* **Smart Semantic Chunking:** Developed a custom layout-analysis heuristic that prevents cutting paragraphs or code blocks in half, ensuring the LLM maintains perfect context during Retrieval-Augmented Generation (RAG).
* **Fault-Tolerant Cloud Bridging:** Integrated Cloudflare Tunnels and a custom polling mechanism to ensure the ASP.NET backend can reliably trigger and track 10+ minute AI generation tasks running dynamically on Google Colab.
* **Biometric Consistency:** Forced strict speaker roles to ensure semantic continuity across long audio tracks.

---

## 🛠️ Setup & Local Execution

Because of the decoupled nature of this project, each service must be started independently.

### Starting the AI Server (Colab/GPU)
1. Open the `/ExternalServices/Copy_of_Doc2Pod_With_TTS_Final` notebook in Google Colab.
2. Add your `SUPABASE_URL` and `SUPABASE_KEY` to Colab Secrets.
3. Run all cells. The final cell will generate a **Cloudflare Tunnel URL** (e.g., `https://random-words.trycloudflare.com`).
4. Copy this URL to configure the Backend connection.

### Starting the Backend (C# ASP.NET)
1. Navigate to the `/Backend` directory.
2. Update `appsettings.json` with your SQL Server connection string and the Cloudflare Tunnel URL from the AI Server.
3. Run migrations: `dotnet ef database update`
4. Start the server: `dotnet run`

### Starting the Frontend (Angular)
1. Navigate to the `/FrontEnd` directory.
2. Install dependencies: `npm install`
3. Serve the application: `ng serve`
4. Open `http://localhost:4200` in your browser.

---
*Developed as a Computer Science Graduation Project at Ain Shams University (Class of 2026).*
