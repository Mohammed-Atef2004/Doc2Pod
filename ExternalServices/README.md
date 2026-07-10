# 🧠 Doc2Pod: AI Inference Engine & API

This repository contains the GPU-accelerated AI microservice for the **Doc2Pod** platform. It is designed to run in a cloud GPU environment (Google Colab - T4) and exposes a thread-safe FastAPI endpoint via Cloudflare Tunnels to communicate with the C# Backend.

Because of the massive VRAM requirements of running Vision, LLM, and TTS models sequentially, this pipeline is engineered with an aggressive **Lazy-Loading and Memory-Sweeping architecture** to prevent Out-Of-Memory (OOM) crashes on a single 16GB GPU.

## 🚀 The AI Pipeline Architecture

When the backend triggers the `/generate` endpoint, the system executes the following pipeline:

1. **Visual Data Extraction:** Uses **PaddleOCR** to extract text/tables, and **Qwen3-VL** (Vision Language Model) to read and describe unparseable charts and diagrams.
2. **Semantic Chunking:** Custom algorithms evaluate page layouts, fonts, and headers to logically group content without splitting paragraphs.
3. **Vectorization & RAG:** Embeds chunks using `BAAI/bge-m3` and stores them in a local **ChromaDB** instance.
4. **Script Generation:** Retrieves relevant context and utilizes a fine-tuned **Qwen LLM** (`Fatma04/Arabic-Podcast-Qwen-16bit`) with strict state-aware prompt engineering to generate a conversational, code-switched (Arabic/English) script.
5. **Multi-Speaker TTS:** Passes the script to a custom **VibeVoice** diffusion model (`MohammedEhab20/vibe-voice-egyptian-cfg35`) to generate realistic Egyptian dialogue.
6. **Audio Compilation:** Merges audio chunks using **FFmpeg** and syncs the final `.mp3` and `.txt` files directly to **Supabase** cloud storage.

## 🛠️ Tech Stack & Models
* **API Framework:** FastAPI, Uvicorn, Cloudflare Tunnels (`cloudflared`)
* **Vision & OCR:** PaddleOCR, Qwen3-VL (4B-Instruct)
* **LLM & RAG:** Qwen (16-bit fine-tune), SentenceTransformers, ChromaDB
* **Audio & TTS:** VibeVoice (Hugging Face), FFmpeg, Soundfile
* **Cloud Storage:** Supabase SDK

## ⚙️ Deployment Instructions (Google Colab)

To spin up the AI server:

1. Upload `Doc2Pod_With_TTS_Final.ipynb` to Google Colab.
2. Go to `Runtime` -> `Change runtime type` and select **T4 GPU**.
3. **Configure Secrets:** Open the Colab Secrets tab (key icon on the left) and add:
   * `SUPABASE_URL`: Your Supabase project URL.
   * `SUPABASE_KEY`: Your Supabase API key.
4. **Run Cells in Order:** * *Note: After running Step 0 and Step 1 (Dependency Installations), you may be prompted to "Restart Session" due to NumPy versioning. Restart the session and continue running the cells.*
5. **Retrieve API Link:** The final cell (`Step 17: Launch Doc2Pod Server`) will output a secure Cloudflare Tunnel link (e.g., `https://random-words.trycloudflare.com`). 
6. Copy this link and paste it into the `appsettings.json` of your C# Backend.

## 📡 Core Endpoints

### `POST /generate`
Triggers the heavy extraction and generation pipeline. The server uses a global lock (`threading.Lock`) to reject concurrent requests and protect GPU memory.
```json
{
  "file_key": "document_name.pdf",
  "mode": 3, 
  "topic": "Optional search query for Mode 1 or 2",
  "start_page": 1,
  "end_page": 10
}
Returns: {"task_id": "uuid-string"}
```
GET /status/{task_id}
A polling endpoint for the backend to check the status of the generation task.
Returns: {"status": "PROCESSING"} OR {"status": "DONE", "script_path": "...", "audio_path": "..."}
