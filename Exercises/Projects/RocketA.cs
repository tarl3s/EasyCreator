namespace EasyCreator.Exercises;

// Markers Q/Qf (pt[,en,es]) are rewritten to L.T/L.F by tools/ConvertMarkers (translations go to Lang/exercises-projects.*.json).

/// <summary>Rockets 1-5: action and reaction, thrust and impulse, motor classes, stability, drag.</summary>
[Exercises("rkt-01-garrafa-pet")]
public sealed class Rkt01 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double M = Ex.Step(r, 0.4, 2, 0.1), m = Ex.Step(r, 0.1, 0.4, 0.05), ve = Ex.Step(r, 10, 40, 2), v = m * ve / M;
                switch (r.Next(3))
                {
                    case 0:
                        return Ex.Num(L.F("Um foguete de garrafa PET, depois de expulsar a água, tem massa {0} kg. A água expulsa tinha {1} kg e saiu a {2} m/s. Partindo do repouso, qual a velocidade do foguete?", Ex.N(M), Ex.N(m), Ex.N(ve)), v, "m/s",
                            L.F("Conservação da quantidade de movimento: M·v = m·vₑ → v = {0}·{1} / {2} = {3} m/s.", Ex.N(m), Ex.N(ve), Ex.N(M), Ex.N(v)));
                    case 1:
                        return Ex.Num(L.F("Um foguete de garrafa PET sem a água tem massa {0} kg. Ele expulsa {1} kg de água e sai do repouso a {2} m/s. Com que velocidade a água foi expulsa?", Ex.N(M), Ex.N(m), Ex.N(v, 3)),
                            ve, "m/s", L.F("M·v = m·vₑ → vₑ = M·v / m = {0}·{1} / {2} = {3} m/s.", Ex.N(M), Ex.N(v, 3), Ex.N(m), Ex.N(ve)));
                    default:
                        return Ex.Num(L.F("Um foguete de garrafa PET tem {0} kg depois de soltar a água. A água sai a {1} m/s e o foguete adquire {2} m/s. Qual era a massa de água expulsa?", Ex.N(M), Ex.N(ve), Ex.N(v, 3)),
                            m, "kg", L.F("M·v = m·vₑ → m = M·v / vₑ = {0}·{1} / {2} = {3} kg.", Ex.N(M), Ex.N(v, 3), Ex.N(ve), Ex.N(m, 3)));
                }
            }
            case Difficulty.Medium:
            {
                double M = Ex.Step(r, 0.5, 2, 0.1), ve = Ex.Step(r, 15, 40, 1), w = M * G;
                double F = Ex.Step(r, 2, 6, 0.5) * w, md = F / ve, m = Ex.Step(r, 0.2, 0.6, 0.05);
                string intro = L.F("Um foguete de água tem {0} kg e expulsa a água a {1} m/s, com vazão de massa de {2} kg/s.", Ex.N(M), Ex.N(ve), Ex.N(md, 3));
                switch (r.Next(3))
                {
                    case 0:
                        return Ex.Num(intro + " " + L.T("Qual o empuxo?"), F, "N",
                            L.F("F = ṁ·vₑ = {0}·{1} = {2} N.", Ex.N(md, 3), Ex.N(ve), Ex.N(F)));
                    case 1:
                        return Ex.Num(intro + " " + L.T("Qual a aceleração inicial na vertical (g = 9,8 m/s²)? Despreze a perda de massa no primeiro instante."),
                            F / M - G, "m/s²", L.F("F = ṁ·vₑ = {0} N; a = F/M − g = {0}/{1} − 9,8 = {2} m/s².", Ex.N(F), Ex.N(M), Ex.N(F / M - G)));
                    default:
                        return Ex.Num(intro + " " + L.F("A garrafa tinha {0} kg de água. Quanto tempo dura a expulsão?", Ex.N(m)), m / md, "s",
                            L.F("t = m / ṁ = {0} / {1} = {2} s.", Ex.N(m), Ex.N(md, 3), Ex.N(m / md, 3)));
                }
            }
            case Difficulty.Hard:
            {
                double dp = Ex.Step(r, 150, 600, 25), dn = Ex.Step(r, 8, 22, 1), A = Pk.Area(dn / 1000), ve = Math.Sqrt(2 * dp * 1000 / Pk.RhoW);
                string intro = L.F("Uma garrafa PET está pressurizada com {0} kPa acima da pressão atmosférica e a água sai por um bocal de {1} mm de diâmetro (água: 1000 kg/m³).", Ex.N(dp), Ex.N(dn));
                switch (r.Next(3))
                {
                    case 0:
                        return Ex.Num(intro + " " + L.T("Qual a velocidade de saída da água no início?"), ve, "m/s",
                            L.F("Bernoulli: ΔP = ½·ρ·v² → v = √(2·ΔP/ρ) = √(2·{0}/1000) = {1} m/s.", Ex.N(dp * 1000), Ex.N(ve)));
                    case 1:
                        return Ex.Num(intro + " " + L.T("Qual a vazão de massa de água no início?"), Pk.RhoW * A * ve, "kg/s",
                            L.F("v = √(2ΔP/ρ) = {0} m/s; A = π·d²/4 = {1} m²; ṁ = ρ·A·v = {2} kg/s.", Ex.N(ve), Ex.N(A, 6), Ex.N(Pk.RhoW * A * ve, 3)));
                    default:
                        return Ex.Num(intro + " " + L.T("Qual o empuxo inicial?"), 2 * dp * 1000 * A, "N",
                            L.F("F = ṁ·v = ρ·A·v² = 2·ΔP·A = 2·{0}·{1} = {2} N.", Ex.N(dp * 1000), Ex.N(A, 6), Ex.N(2 * dp * 1000 * A)));
                }
            }
            default:
            {
                double M0 = Ex.Step(r, 0.8, 2, 0.1), m = Math.Round(M0 * Ex.Step(r, 0.25, 0.5, 0.05), 2), ve = Ex.Step(r, 14, 30, 1);
                double dv = ve * Math.Log(M0 / (M0 - m)), h = dv * dv / (2 * G);
                return Ex.Num(L.F("Um foguete de água pesa {0} kg com a garrafa cheia, dos quais {1} kg são água. A água sai a {2} m/s (constante). Supondo expulsão instantânea e sem arrasto, qual a altura máxima atingida (g = 9,8 m/s²)?", Ex.N(M0), Ex.N(m), Ex.N(ve)),
                    h, "m", L.F("Δv = vₑ·ln(M₀/(M₀−m)) = {0}·ln({1}/{2}) = {3} m/s. Depois, h = Δv²/(2g) = {3}²/19,6 = {4} m.", Ex.N(ve), Ex.N(M0), Ex.N(M0 - m), Ex.N(dv), Ex.N(h)));
            }
        }
    }
}

[Exercises("rkt-02-empuxo-impulso")]
public sealed class Rkt02 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double F = Ex.Step(r, 5, 60, 1), t = Ex.Step(r, 0.4, 3, 0.1), I = F * t;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um motor de foguete modelo dá um empuxo constante de {0} N durante {1} s. Qual o impulso total?", Ex.N(F), Ex.N(t)), I, "N·s", L.F("I = F·t = {0}·{1} = {2} N·s.", Ex.N(F), Ex.N(t), Ex.N(I)));
                    case 1: return Ex.Num(L.F("Um motor tem impulso total de {0} N·s e empuxo constante de {1} N. Quanto tempo ele queima?", Ex.N(I), Ex.N(F)), t, "s", L.F("t = I / F = {0} / {1} = {2} s.", Ex.N(I), Ex.N(F), Ex.N(t)));
                    default: return Ex.Num(L.F("Um motor tem impulso total de {0} N·s e queima por {1} s com empuxo constante. Qual o empuxo?", Ex.N(I), Ex.N(t)), F, "N", L.F("F = I / t = {0} / {1} = {2} N.", Ex.N(I), Ex.N(t), Ex.N(F)));
                }
            }
            case Difficulty.Medium:
            {
                double mp = Ex.Step(r, 10, 120, 5) / 1000, isp = Ex.Step(r, 70, 200, 5), I = mp * isp * G;
                double t = Ex.Step(r, 0.5, 4, 0.1);
                string intro = L.F("Um motor sólido queima {0} g de propelente com impulso específico de {1} s (g₀ = 9,8 m/s²).", Ex.N(mp * 1000), Ex.N(isp));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Qual o impulso total?"), I, "N·s", L.F("I = m·Isp·g₀ = {0}·{1}·9,8 = {2} N·s.", Ex.N(mp, 3), Ex.N(isp), Ex.N(I)));
                    case 1: return Ex.Num(intro + " " + L.F("Se a queima dura {0} s, qual o empuxo médio?", Ex.N(t)), I / t, "N", L.F("I = {0} N·s; F = I / t = {0} / {1} = {2} N.", Ex.N(I), Ex.N(t), Ex.N(I / t)));
                    default: return Ex.Num(L.F("Um motor com impulso específico de {0} s precisa entregar {1} N·s. Que massa de propelente é necessária? (g₀ = 9,8 m/s²)", Ex.N(isp), Ex.N(I)), mp * 1000, "g", L.F("m = I / (Isp·g₀) = {0} / ({1}·9,8) = {2} kg = {3} g.", Ex.N(I), Ex.N(isp), Ex.N(mp, 4), Ex.N(mp * 1000)));
                }
            }
            case Difficulty.Hard:
            {
                double f1 = Ex.Step(r, 20, 60, 5), t1 = Ex.Step(r, 0.1, 0.4, 0.1), f2 = Ex.Step(r, 8, 18, 1), t2 = Ex.Step(r, 0.8, 2.5, 0.1);
                double I = f1 * t1 + f2 * t2, Tt = t1 + t2;
                string intro = L.F("A curva de empuxo de um motor é simplificada em dois trechos: {0} N durante {1} s (pico inicial) e depois {2} N durante {3} s.", Ex.N(f1), Ex.N(t1), Ex.N(f2), Ex.N(t2));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Qual o impulso total?"), I, "N·s", L.F("I = {0}·{1} + {2}·{3} = {4} N·s.", Ex.N(f1), Ex.N(t1), Ex.N(f2), Ex.N(t2), Ex.N(I)));
                    case 1: return Ex.Num(intro + " " + L.T("Qual o empuxo médio durante toda a queima?"), I / Tt, "N", L.F("I = {0} N·s; T = {1} s; F médio = I / T = {2} N.", Ex.N(I), Ex.N(Tt), Ex.N(I / Tt)));
                    default:
                        double isp = Ex.Step(r, 80, 200, 5), mp = I / (isp * G);
                        return Ex.Num(intro + " " + L.F("Com impulso específico de {0} s, qual a massa de propelente queimada? (g₀ = 9,8 m/s²)", Ex.N(isp)), mp * 1000, "g",
                            L.F("I = {0} N·s; m = I / (Isp·g₀) = {0} / ({1}·9,8) = {2} g.", Ex.N(I), Ex.N(isp), Ex.N(mp * 1000)));
                }
            }
            default:
            {
                double M = Ex.Step(r, 0.3, 1.2, 0.05), F = Ex.Step(r, 3, 6, 0.5) * M * G, t = Ex.Step(r, 0.8, 2.5, 0.1);
                double a = F / M - G, v = a * t, hb = 0.5 * a * t * t, ha = hb + v * v / (2 * G);
                return Ex.Num(L.F("Um foguete de {0} kg (massa constante, aproximação) é lançado na vertical com empuxo constante de {1} N por {2} s. Depois o motor apaga e ele sobe por inércia, sem arrasto. Qual a altura máxima (g = 9,8 m/s²)?", Ex.N(M), Ex.N(F), Ex.N(t)),
                    ha, "m", L.F("a = F/M − g = {0} m/s²; v = a·t = {1} m/s; h₁ = ½·a·t² = {2} m; h₂ = v²/(2g) = {3} m; h = {4} m.", Ex.N(a), Ex.N(v), Ex.N(hb), Ex.N(v * v / (2 * G)), Ex.N(ha)));
            }
        }
    }
}

[Exercises("rkt-03-classes-empuxo-peso")]
public sealed class Rkt03 : IExerciseGenerator
{
    static readonly (string C, double Lo, double Hi)[] Classes = { ("A", 1.26, 2.5), ("B", 2.51, 5), ("C", 5.01, 10), ("D", 10.01, 20), ("E", 20.01, 40), ("F", 40.01, 80) };

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double M = Ex.Step(r, 0.1, 1.5, 0.05), tw = Ex.Step(r, 3, 12, 0.5), F = tw * M * G;
                string intro = L.F("Um foguete modelo tem massa de {0} kg na decolagem (g = 9,8 m/s²).", Ex.N(M));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.F("Seu motor dá {0} N de empuxo. Qual a relação empuxo/peso?", Ex.N(F)), F / (M * G), "", L.F("Peso = M·g = {0} N; T/W = {1} / {0} = {2}.", Ex.N(M * G), Ex.N(F), Ex.N(tw)));
                return Ex.Num(intro + " " + L.F("Para ter relação empuxo/peso igual a {0}, qual deve ser o empuxo do motor?", Ex.N(tw)), F, "N", L.F("F = (T/W)·M·g = {0}·{1}·9,8 = {2} N.", Ex.N(tw), Ex.N(M), Ex.N(F)));
            }
            case Difficulty.Medium:
            {
                double tw = Ex.Step(r, 3, 10, 0.5), M = Ex.Step(r, 0.2, 1.2, 0.05), F = tw * M * G;
                switch (r.Next(3))
                {
                    case 0:
                        var c = Classes[r.Next(Classes.Length)];
                        double I = Math.Round(c.Lo + (c.Hi - c.Lo) * Ex.Step(r, 0.1, 0.9, 0.1), 1), t = Ex.Step(r, 0.5, 3, 0.1), Fm = I / t;
                        var wrong = Classes.Where(x => x.C != c.C).Select(x => x.C).ToList();
                        return Ex.Choice(r, L.F("Um motor entrega empuxo médio de {0} N durante {1} s. Pela tabela (A: até 2,5 N·s; B: até 5; C: até 10; D: até 20; E: até 40; F: até 80; cada letra dobra a anterior), de que classe ele é?", Ex.N(Fm), Ex.N(t)),
                            c.C, wrong, L.F("I = F·t = {0}·{1} = {2} N·s, que fica na classe {3}.", Ex.N(Fm), Ex.N(t), Ex.N(Fm * t), c.C));
                    case 1:
                        return Ex.Num(L.F("Um foguete de {0} kg decola com um motor de {1} N. Qual a aceleração inicial? (g = 9,8 m/s²)", Ex.N(M), Ex.N(F)),
                            F / M - G, "m/s²", L.F("a = F/M − g = {0}/{1} − 9,8 = {2} m/s² (equivale a T/W − 1 = {3} g).", Ex.N(F), Ex.N(M), Ex.N(F / M - G), Ex.N(tw - 1)));
                    default:
                        return Ex.Num(L.F("Um motor dá {0} N de empuxo. Qual a maior massa de decolagem para que a relação empuxo/peso seja pelo menos {1}? (g = 9,8 m/s²)", Ex.N(F), Ex.N(tw)),
                            M, "kg", L.F("M = F / ((T/W)·g) = {0} / ({1}·9,8) = {2} kg.", Ex.N(F), Ex.N(tw), Ex.N(M)));
                }
            }
            case Difficulty.Hard:
            {
                double M = Ex.Step(r, 0.2, 1, 0.05), tw = Ex.Step(r, 4, 12, 0.5), F = tw * M * G, Lr = Ex.Step(r, 0.8, 2, 0.1), a = F / M - G, v = Math.Sqrt(2 * a * Lr);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um foguete de {0} kg com motor de {1} N sobe por uma haste de lançamento vertical de {2} m. Qual sua velocidade ao sair da haste? (g = 9,8 m/s², massa constante)", Ex.N(M), Ex.N(F), Ex.N(Lr)),
                        v, "m/s", L.F("a = F/M − g = {0} m/s²; v = √(2·a·L) = √(2·{0}·{1}) = {2} m/s.", Ex.N(a), Ex.N(Lr), Ex.N(v)));
                double t = v / a;
                return Ex.Num(L.F("Um foguete de {0} kg com motor de {1} N sobe por uma haste de lançamento vertical de {2} m. Quanto tempo ele leva para percorrer a haste? (g = 9,8 m/s², massa constante)", Ex.N(M), Ex.N(F), Ex.N(Lr)),
                    t, "s", L.F("a = F/M − g = {0} m/s²; L = ½·a·t² → t = √(2L/a) = √(2·{1}/{0}) = {2} s.", Ex.N(a), Ex.N(Lr), Ex.N(t, 3)));
            }
            default:
            {
                double Ms = Ex.Step(r, 0.2, 0.8, 0.05), mm = Ex.Step(r, 40, 120, 5) / 1000, mp = Math.Round(mm * Ex.Step(r, 0.5, 0.7, 0.05), 3), F = Ex.Step(r, 10, 40, 1);
                double m0 = Ms + mm, mf = m0 - mp, tw0 = F / (m0 * G), twf = F / (mf * G);
                string intro = L.F("Um foguete tem {0} kg sem o motor. O motor pesa {1} g com {2} g de propelente e dá {3} N constantes (g = 9,8 m/s²).", Ex.N(Ms), Ex.N(mm * 1000), Ex.N(mp * 1000), Ex.N(F));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual a relação empuxo/peso logo antes do fim da queima, com o propelente consumido?"),
                        twf, "", L.F("Massa final = {0} + {1} − {2} = {3} kg; T/W = {4} / ({3}·9,8) = {5}.", Ex.N(Ms), Ex.N(mm, 3), Ex.N(mp, 3), Ex.N(mf, 3), Ex.N(F), Ex.N(twf)));
                return Ex.Num(intro + " " + L.T("Quantas vezes a relação empuxo/peso no fim da queima é maior que na decolagem?"),
                    twf / tw0, "", L.F("T/W inicial = {0}/({1}·9,8) = {2}; final = {0}/({3}·9,8) = {4}; razão = {1}/{3} = {5}.", Ex.N(F), Ex.N(m0, 3), Ex.N(tw0), Ex.N(mf, 3), Ex.N(twf), Ex.N(m0 / mf, 3)));
            }
        }
    }
}

[Exercises("rkt-04-estabilidade")]
public sealed class Rkt04 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double cal = Ex.Step(r, 3, 8, 0.5); // body diameter, cm
        switch (d)
        {
            case Difficulty.Easy:
            {
                double cg = Ex.Step(r, 30, 60, 1), cp = cg + Ex.Step(r, 0.5, 3, 0.5) * cal, marg = (cp - cg) / cal;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um foguete com {0} cm de diâmetro tem o CG a {1} cm da ponta do nariz e o CP a {2} cm. Qual a margem estática em calibres?", Ex.N(cal), Ex.N(cg), Ex.N(cp)), marg, "cal", L.F("Margem = (CP − CG) / diâmetro = ({0} − {1}) / {2} = {3} calibres.", Ex.N(cp), Ex.N(cg), Ex.N(cal), Ex.N(marg)));
                    case 1: return Ex.Num(L.F("Um foguete com {0} cm de diâmetro tem o CG a {1} cm da ponta. Para uma margem estática de {2} calibres, a que distância da ponta deve estar o CP?", Ex.N(cal), Ex.N(cg), Ex.N(marg)), cp, "cm", L.F("CP = CG + margem·diâmetro = {0} + {1}·{2} = {3} cm.", Ex.N(cg), Ex.N(marg), Ex.N(cal), Ex.N(cp)));
                    default:
                        double cp2 = cg - Ex.Step(r, 0.5, 2, 0.5) * cal;
                        return Ex.Choice(r, L.F("Um foguete tem o CG a {0} cm da ponta do nariz e o CP a {1} cm. O que acontece em voo?", Ex.N(cg), Ex.N(cp2)),
                            L.T("É instável: o CP está à frente do CG e o foguete tende a girar."),
                            new[] { L.T("É estável: o CP está atrás do CG."), L.T("É estável porque o CG está longe do nariz."), L.T("Voa reto, a posição do CP não importa.") },
                            L.T("Para ser estável o CG precisa ficar na frente do CP (mais perto do nariz). Aqui o CP (onde a força do ar atua) fica antes do CG, então qualquer desvio aumenta."));
                }
            }
            case Difficulty.Medium:
            {
                double Lr = Ex.Step(r, 40, 80, 2), xn = Ex.Step(r, 3, 6, 0.5), cnf = Ex.Step(r, 2, 6, 0.5), xf = Lr - Ex.Step(r, 3, 8, 1);
                double cn = 2, cp = (cn * xn + cnf * xf) / (cn + cnf);
                string intro = L.F("Um foguete tem duas contribuições de força lateral: o nariz (coeficiente {0}, aplicada a {1} cm da ponta) e as aletas (coeficiente {2}, aplicada a {3} cm da ponta).", Ex.N(cn), Ex.N(xn), Ex.N(cnf), Ex.N(xf));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Onde fica o CP (média ponderada pelos coeficientes)?"), cp, "cm", L.F("CP = (C₁·x₁ + C₂·x₂) / (C₁ + C₂) = ({0}·{1} + {2}·{3}) / ({0} + {2}) = {4} cm.", Ex.N(cn), Ex.N(xn), Ex.N(cnf), Ex.N(xf), Ex.N(cp)));
                double cg = cp - cal * Ex.Step(r, 1, 2.5, 0.5);
                return Ex.Num(intro + " " + L.F("O diâmetro do corpo é {0} cm e o CG está a {1} cm da ponta. Qual a margem estática em calibres?", Ex.N(cal), Ex.N(cg)),
                    (cp - cg) / cal, "cal", L.F("CP = {0} cm; margem = (CP − CG) / d = ({0} − {1}) / {2} = {3} calibres.", Ex.N(cp), Ex.N(cg), Ex.N(cal), Ex.N((cp - cg) / cal)));
            }
            case Difficulty.Hard:
            {
                double M = Ex.Step(r, 0.2, 0.8, 0.05), cg = Ex.Step(r, 40, 60, 1), cp = cg + Ex.Step(r, -0.2, 0.4, 0.2) * cal, mt = Ex.Step(r, 1.5, 2.5, 0.5), xt = cp - mt * cal, xn = Ex.Step(r, 2, 8, 1);
                double m = M * (cg - xt) / (xt - xn);
                return Ex.Num(L.F("Um foguete de {0} kg (sem lastro) tem o CG a {1} cm da ponta, o CP a {2} cm e {3} cm de diâmetro. Quanto de massa de lastro colocado a {4} cm da ponta é preciso para a margem estática ser {5} calibres?", Ex.N(M), Ex.N(cg), Ex.N(cp), Ex.N(cal), Ex.N(xn), Ex.N(mt)),
                    m, "kg", L.F("CG desejado = CP − margem·d = {0} − {1}·{2} = {3} cm. Média ponderada: (M·CG + m·x) / (M + m) = {3} → m = M·(CG − {3}) / ({3} − x) = {4} kg.", Ex.N(cp), Ex.N(mt), Ex.N(cal), Ex.N(xt), Ex.N(m, 3)), 3);
            }
            default:
            {
                double M = Ex.Step(r, 0.4, 1, 0.05), mp = Math.Round(M * Ex.Step(r, 0.15, 0.3, 0.05), 3), cg = Ex.Step(r, 40, 55, 1), xp = Ex.Step(r, 60, 75, 1), cp = cg + Ex.Step(r, 1, 2, 0.5) * cal;
                double cgf = (M * cg - mp * xp) / (M - mp), m0 = (cp - cg) / cal, mf = (cp - cgf) / cal;
                return Ex.Num(L.F("Um foguete carregado de {0} kg tem o CG a {1} cm da ponta e o CP a {2} cm (diâmetro {3} cm). Seu propelente, de {4} kg, fica no motor a {5} cm da ponta. Qual a margem estática, em calibres, depois que o propelente acaba?", Ex.N(M), Ex.N(cg), Ex.N(cp), Ex.N(cal), Ex.N(mp, 3), Ex.N(xp)),
                    mf, "cal", L.F("CG novo = (M·CG − m·x) / (M − m) = ({0}·{1} − {2}·{3}) / {4} = {5} cm (o CG anda para a frente, pois o peso da cauda sumiu). Margem = ({6} − {5}) / {7} = {8} calibres (antes: {9}).", Ex.N(M), Ex.N(cg), Ex.N(mp, 3), Ex.N(xp), Ex.N(M - mp, 3), Ex.N(cgf), Ex.N(cp), Ex.N(cal), Ex.N(mf), Ex.N(m0)));
            }
        }
    }
}

[Exercises("rkt-05-arrasto")]
public sealed class Rkt05 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double dia = Ex.Step(r, 3, 12, 0.5), A = Pk.Area(dia / 100), cd = Ex.Step(r, 0.3, 0.8, 0.05), v = Ex.Step(r, 20, 150, 5), k = 0.5 * Pk.Rho * cd * A;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double Fd = k * v * v;
                string intro = L.F("Um foguete com {0} cm de diâmetro tem Cd = {1} e voa a {2} m/s no ar (ρ = 1,2 kg/m³).", Ex.N(dia), Ex.N(cd), Ex.N(v));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual a força de arrasto?"), Fd, "N", L.F("A = π·d²/4 = {0} m²; F = ½·ρ·Cd·A·v² = 0,5·1,2·{1}·{0}·{2}² = {3} N.", Ex.N(A, 5), Ex.N(cd), Ex.N(v), Ex.N(Fd)));
                return Ex.Num(intro + " " + L.T("Se a velocidade dobrar, qual será o arrasto?"), 4 * Fd, "N", L.F("O arrasto cresce com v²: F(v) = {0} N, então F(2v) = 4·{0} = {1} N.", Ex.N(Fd), Ex.N(4 * Fd)));
            }
            case Difficulty.Medium:
            {
                double Fd = k * v * v, M = Ex.Step(r, 0.2, 1.5, 0.05);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Em um túnel de vento, um foguete de {0} cm de diâmetro sofre {1} N de arrasto a {2} m/s (ρ = 1,2 kg/m³). Qual o coeficiente de arrasto Cd?", Ex.N(dia), Ex.N(Fd, 3), Ex.N(v)), cd, "", L.F("A = {0} m²; Cd = 2·F / (ρ·A·v²) = 2·{1} / (1,2·{0}·{2}²) = {3}.", Ex.N(A, 5), Ex.N(Fd, 3), Ex.N(v), Ex.N(cd)));
                    case 1: return Ex.Num(L.F("Um foguete de {0} kg, {1} cm de diâmetro e Cd = {2} sobe a {3} m/s com o motor apagado. Qual a desaceleração total (arrasto + gravidade)? (g = 9,8; ρ = 1,2 kg/m³)", Ex.N(M), Ex.N(dia), Ex.N(cd), Ex.N(v)), G + Fd / M, "m/s²", L.F("Arrasto = {0} N; a = g + F/M = 9,8 + {0}/{1} = {2} m/s².", Ex.N(Fd), Ex.N(M), Ex.N(G + Fd / M)));
                    default:
                        double P = Fd * v;
                        return Ex.Num(L.F("Um foguete de {0} cm de diâmetro e Cd = {1} voa a {2} m/s. Qual a potência dissipada pelo arrasto? (ρ = 1,2 kg/m³)", Ex.N(dia), Ex.N(cd), Ex.N(v)), P, "W", L.F("F = ½ρCdAv² = {0} N; P = F·v = {0}·{1} = {2} W.", Ex.N(Fd), Ex.N(v), Ex.N(P)));
                }
            }
            case Difficulty.Hard:
            {
                double M = Ex.Step(r, 0.3, 1.5, 0.05), vt = Math.Sqrt(M * G / k), f = Ex.Step(r, 1.5, 3, 0.5);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um foguete de {0} kg com {1} cm de diâmetro e Cd = {2} cai de bico para baixo, sem paraquedas. Qual a velocidade terminal (arrasto = peso)? (g = 9,8; ρ = 1,2 kg/m³)", Ex.N(M), Ex.N(dia), Ex.N(cd)), vt, "m/s", L.F("½ρCdA·v² = M·g → v = √(M·g / (½ρCdA)) = √({0}·9,8 / {1}) = {2} m/s.", Ex.N(M), Ex.N(k, 5), Ex.N(vt)));
                return Ex.Num(L.F("Um foguete tem Cd = {0} e {1} cm de diâmetro. Uma ponta de nariz melhor reduz o Cd em {2}%. Se o arrasto original a {3} m/s era F₀, qual é o arrasto novo em percentual de F₀? (responda só o número)", Ex.N(cd), Ex.N(dia), Ex.N(f * 10), Ex.N(v)), 100 - f * 10, "%", L.F("O arrasto é proporcional ao Cd (mesma área e velocidade): sobra 100 − {0} = {1}% de F₀.", Ex.N(f * 10), Ex.N(100 - f * 10)));
            }
            default:
            {
                double M = Ex.Step(r, 0.3, 1.5, 0.05), vb = Ex.Step(r, 60, 200, 5);
                double h = M / (2 * k) * Math.Log(1 + k * vb * vb / (M * G)), hv = vb * vb / (2 * G);
                return Ex.Num(L.F("Um foguete de {0} kg, {1} cm de diâmetro e Cd = {2} termina a queima a {3} m/s, na vertical. Sem motor, quanto ainda sobe contra o ar e a gravidade? (ρ = 1,2 kg/m³, g = 9,8 m/s²; use h = M/(2k)·ln(1 + k·v²/(M·g)), com k = ½ρCdA)", Ex.N(M), Ex.N(dia), Ex.N(cd), Ex.N(vb)),
                    h, "m", L.F("k = ½·ρ·Cd·A = {0} kg/m; h = {1}/(2·{0})·ln(1 + {0}·{2}²/({1}·9,8)) = {3} m (sem arrasto seria {4} m).", Ex.N(k, 5), Ex.N(M), Ex.N(vb), Ex.N(h), Ex.N(hv)));
            }
        }
    }
}
