# EasyCreator

**Aprenda programação, robótica e engenharia — da primeira linha de código ao nível de faculdade — com desafios que ficam mais difíceis com o tempo.**

[English](README.md) · Disponível em Português, English e Español.

[![Baixar para Windows](https://img.shields.io/badge/Baixar-Windows%20x64-18B98A?style=for-the-badge)](https://github.com/tarl3s/EasyCreator/releases/latest/download/EasyCreator-win-x64.zip)

Descompacte em qualquer pasta e abra o `EasyCreator.exe`. Não precisa instalar nada, nem o .NET.

## O que tem dentro

| Área | Trilhas | Como você pratica |
|---|---|---|
| 💻 Programação | Python, JavaScript, C, C# | Escreva o código no app; ele é corrigido por testes (alguns ocultos). Roda offline ou com Python/Node/gcc/.NET instalados no seu computador. |
| 🤖 Robótica | Arduino | Um Arduino Uno simulado: LEDs, botões, potenciômetros, servos, ultrassom, motores, LCD e um robô seguidor de linha. |
| ⚛️ Física | Do movimento à física moderna | Laboratórios: projétil com resistência do ar, plano inclinado, molas, pêndulo, colisões, órbitas, lentes, gás ideal. |
| ⚡ Engenharia Elétrica | De Ohm a sistemas de potência | Simulador de circuitos (CC, transientes, fasores CA, trifásico) com multímetro e osciloscópio virtuais. |
| 🔥 Metalurgia e Solda | Do átomo ao cordão | Simulador de solda (eletrodo revestido, MIG/MAG, TIG), tratamento térmico e ensaio de tração. **Soldar de verdade exige prática com instrutor qualificado e EPI adequado.** |
| 🚀 Projetos | Foguetes, submarinos, carros, aviões e helicópteros, drones | Projete e teste nos simuladores, do básico a um projeto completo. |

Cada trilha tem 20 lições, do nível 1 (iniciante) ao 5 (faculdade). As lições liberam em ordem e o progresso fica salvo.

**🧪 Sandbox** — modo livre: programe em qualquer uma das quatro linguagens, ou monte seu próprio circuito de Arduino (escolhendo peças e pinos) e simule.

## Compilar o código

Windows com o .NET 8 SDK:

```bash
dotnet build
dotnet run
dotnet run -- --test        # todas as baterias de teste, log em %TEMP%\easycreator_test.log
```

## Segurança

Os simuladores ensinam as ideias; não substituem treinamento real. Rede elétrica, solda, motores de foguete, baterias LiPo e hélices podem ferir ou matar. Siga os avisos de segurança de cada lição e as leis do seu país.

## Licença

MIT — veja [LICENSE](LICENSE).
