# Atualizando objetos usando DOTS sem ECS
[DOTS](https://unity.com/dots) - a Stack de Tecnologias Orientadas a Dados da Unity - é uma combinação de tecnologias que nos permite construir jogos com uma arquitetura orientada a dados.
Quando usada corretamente, os dados podem ser processados com alta performance, se beneficiando de execução paralela, compilação de C# para código nativo altamente otimizado usando o compilador [Burst](https://docs.unity3d.com/Packages/com.unity.burst@latest?subfolder=/manual/index.html), e muito mais.

A solução oficial para execução de métodos Update todo frame em DOTS é utilizando o pacote [Entities](https://docs.unity3d.com/Packages/com.unity.entities@latest?subfolder=/manual/index.html) - um *framework* que implementa a arquitetura [Entidade Componente Sistema (ECS)](https://unity.com/ecs).
Assim como descrito na documentação do [workflow ECS](https://docs.unity3d.com/Packages/com.unity.entities@1.2/manual/ecs-workflow-intro.html), o jeito padrão de trabalhar com ECS é criando uma subcena, criando GameObjects e MonoBehaviours normalmente, depois criando classes geradoras (chamadas em inglês de *bakers*) que vão gerar componentes ECS a partir dos componentes dos GameObjects.
Após o processo de geração das entidades e componentes ECS, também chamado de *bake*, não há mais os GameObjects e MonoBehaviours originais na cena, então muitos dos fluxos de desenvolvimento e pacotes que estamos acostumados a usar em Unity não são suportados quando trabalhamos com objetos ECS.

Se você ainda não usa ECS no seu projeto, pode ser bem difícil refatorar a lógica de *gameplay* existente para funcionar com a arquitetura ECS.
Algumas vezes, tudo o que queremos é executar lógica de Update em MonoBehaviours de modo paralelo em *threads* de *background*, mas continuar utilizando GameObjects.
Nesses casos, por exemplo para mover centenas de balas em um jogo de nave estilo *Shoot 'em Up* e vários outros casos, usar o pacote [Update Manager](https://github.com/gilzoide/unity-update-manager) e seu suporte ao Job System é bem mais simples do que migrar para ECS, principalmente se estiver trabalhando com componentes que já existem, ao invés de implementá-los do zero.


## Seguindo em frente (usando Jobs C#)
Usar Jobs de Update é bem simples.
Após instalar o pacote [Update Manager](https://github.com/gilzoide/unity-update-manager), o fluxo é o seguinte:
1. Crie um tipo *struct* que implementa a interface `IUpdateTransformJob`.
   Jobs podem atualizar o `Transform` usando a instância de `TransformAccess` passada para eles.
2. Herdar `AJobBehaviour<>` ao invés de `MonoBehaviour`: a única implementação necessária é a propriedade `InitialJobData`, que representa o estado inicial do Job.

Primeiramente, vamos criar um Job que move um objeto para frente:
```cs
using Gilzoide.UpdateManager.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

// 1. Crie um tipo struct que implementa `IUpdateTransformJob`
public struct SegueEmFrenteJob : IUpdateTransformJob
{
    // 2. (opcional) Defina os parâmetros necessários
    public float Velocidade;

    // 3. Implemente o método Execute
    public void Execute(TransformAccess transformAccess)
    {
        // Threads de background não podem acessar `Time.deltaTime`,
        // então precisamos usar `UpdateJobTime.deltaTime` no lugar
        float deltaTime = UpdateJobTime.deltaTime;

        // `TransformAccess` não possui um método `Translate`,
        // então precisamos atualizar sua posição diretamente
        Vector3 direcao = transformAccess.rotation * Vector3.forward;
        transformAccess.position += Velocidade * deltaTime * direcao;
    }
}
```

Por último, mas não menos importante, vamos definir um componente que executa nosso `SegueEmFrenteJob` todo frame como seu método de Update:
```cs
using Gilzoide.UpdateManager.Jobs;
using UnityEngine;

// 1. Herde `AJobBehaviour<>` e seu componente será atualizado
// automaticamente usando o Job System enquanto estive ativado.
public class SegueEmFrenteBehaviour : AJobBehaviour<SegueEmFrenteJob>
{
    // 2. (opcional) Defina os parâmetros necessários
    public float Velocidade = 1;

    // 3. Defina o estado utilizado ao inicializar um novo Job
    public override SegueEmFrenteJob InitialJobData => new SegueEmFrenteJob
    {
        Velocidade = this.Velocidade,
    };
}
```

E é isso aí, o fluxo básico está pronto: quando `SegueEmFrenteBehaviour` estiver ativado, ele será atualizado todo frame usando o Job System da Unity.
Quando desativado, o Job de Update não será mais executado.
Adicione esse componente a um GameObject na sua cena, dê Play e você verá seu objeto movendo para frente.


## Paralelismo
O pacote Update Manager executa todas as instâncias de um mesmo Job em uma única execução de Job paralelizável.
Não há garantia que o Job System executará os Jobs em paralelo, dado que há limites no número de *threads* prealocadas para executar Jobs.
Além disso, dependendo do número de objetos, é possível que todos eles sejam executados de uma só vez por uma única *thread*.

Para Jobs com `TransformAccess`, se os Transforms existirem em hierarquias diferentes, isto é, não compartilharem um mesmo GameObject como raiz, os Jobs são paralelizáveis.
Se seu Job somente lê os valores do `TransformAccess`, mas nunca os modifica, pode marcar seu tipo *struct* com o atributo `[ReadOnlyTransformAccess]` para que mesmo Transforms que compatilham um mesmo GameObject possam ser processados em paralelo.


## Sincronizando dados
Agora que nosso Job de Update é uma instância diferente em memória do que o MonoBehaviour que a criou, seus dados podem ficar dessincronizados.
Por examplo, se mudarmos o valor de `Velocidade` de um componente `SegueEmFrenteBehaviour` que já estava ativo e executando, o Job ainda usará o valor anterior.

Para sincronizar dados entre um Job de Update e a instância que a criou, precisamos implementar a interface `IJobDataSynchronizer<>`.
Essa interface declara somente um método: `void SyncJobData(ref T jobData)`.
Note que o parâmetro é declarado como `ref T`, o que significa que podemos modificar os dados do Job diretamente, mesmo sendo um tipo *struct*.

Vamos implementar sincronização de dados no nosso componente `SegueEmFrenteBehaviour`, para atualizar o valor de `Velocidade` no Job quando o modificarmos no Inspector:
```cs
using Gilzoide.UpdateManager.Jobs;
using UnityEngine;

public class SegueEmFrenteBehaviour : AJobBehaviour<SegueEmFrenteJob>,
    // 1. Declare que a classe implementa `IJobDataSynchronizer<>`
    IJobDataSynchronizer<SegueEmFrenteJob>
{
    public float Velocidade = 1;

    public override SegueEmFrenteJob InitialJobData => new SegueEmFrenteJob
    {
        Velocidade = this.Velocidade,
    };

    // 2. Implemente a sincronização de dados.
    // Neste caso, basta copiar o valor de `Velocidade`.
    public void SyncJobData(ref SegueEmFrenteJob jobData)
    {
        jobData.Velocidade = this.Velocidade;
    }
}
```

Prontinho.
Agora, o objeto sincronizará os dados do Job automaticamente quando modificarmos o valor de `Velocidade` no Inspector, utilizando a mensagem `OnValidate`.
Para executar uma sincronização de dados manualmente, basta chamar `this.SynchronizeJobDataOnce()`.
Se quiser executar a sincronização de dados todos os frames, chame `this.RegisterInManager(true)`.

Por exemplo, para modificar a `Velocidade` em código enquanto o Job está sendo executado, podemos usar o método `SetVelocidade` a seguir:
```cs
using Gilzoide.UpdateManager.Jobs;
using UnityEngine;

public class SegueEmFrenteBehaviour : AJobBehaviour<SegueEmFrenteJob>,
    IJobDataSynchronizer<SegueEmFrenteJob>
{
    public float Velocidade = 1;

    public override SegueEmFrenteJob InitialJobData => new SegueEmFrenteJob
    {
        Velocidade = this.Velocidade,
    };

    public void SyncJobData(ref SegueEmFrenteJob jobData)
    {
        jobData.Velocidade = this.Velocidade;
    }

    // Troca o valor de `Velocidade`.
    // Se o valor for diferente do anterior, sincroniza os dados com o Job.
    public void SetVelocidade(float velocidade)
    {
        if (this.Velocidade != velocidade)
        {
            this.Velocidade = velocidade;
            this.SynchronizeJobDataOnce();
        }
    }
}
```


## Usando Burst
Para usar o compilador Burst e seus Jobs de Update serem compilados para código nativo altamente otimizado, basta modificar a definição do seu Job implementando a interface `IBurstUpdateTransformJob<>` ao invés de `IUpdateTransformJob`:
```cs
using Gilzoide.UpdateManager.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public struct SegueEmFrenteJob : IBurstUpdateTransformJob<BurstUpdateTransformJob<SegueEmFrenteJob>>
                              // ^ aqui
{
    public float Velocidade;

    public void Execute(TransformAccess transformAccess)
    {
        float deltaTime = UpdateJobTime.deltaTime;
        Vector3 direcao = transformAccess.rotation * Vector3.forward;
        transformAccess.position += Velocidade * deltaTime * direcao;
    }
}
```

Note que é necessário escrever `BurstUpdateTransformJob<SegueEmFrenteJob>` no código do seu projeto explicitamente ou o copilador Burst não será capaz de compilar seu Job.
É por isso que a interface é `IBurstUpdateTransformJob<>` e não `IBurstUpdateTransformJob`, somente pra cumprir essa formalidade.


## Outras funcionalidades maneiras
O pacote Update Manager também possui outras funcionalidades bacanas relacionadas a DOTS/Jobs:
- Qualquer classe de C# pode ser atualizada usando Jobs, não precisa herdar de MonoBehaviour.
  Implemente `IJobUpdatable` ou `ITransformJobUpdatable`, comece a rodar updates com `this.RegisterInManaget()` e pronto.
  Não esqueça de parar a execução dos updates usando `this.UnregisterInManager()` quando necessário.
- É possível criar Jobs sem um `TransformAccess`, basta implementar `IUpdateJob` ao invés de `IUpdateTransformJob`.
- É possível definir dependências entre Jobs, de modo que um Job somente é executado após o outro terminar.
  Veja a cena [Follow Target](../Samples~/FollowTarget/) para um exemplo de uso.

Para um exemplo de classes C# puras sendo atualizadas todo frame usando Jobs sem `TransformAccess` e com sincronização de dados, veja o pacote [Tween Jobs](https://github.com/gilzoide/unity-tween-jobs).