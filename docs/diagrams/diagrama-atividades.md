# Diagrama de Atividades — Processo de Pedido de Compras

```mermaid
flowchart TD
    Start([Início]) --> Create[Elaborador cria pedido de compra com itens]
    Create --> CalcTotal[Sistema calcula valor total do pedido]
    CalcTotal --> Submit[Elaborador envia pedido para aprovação]
    Submit --> DefineChain[Sistema define cadeia de aprovação pela alçada]
    DefineChain --> Supplies{Suprimentos analisa pedido}

    Supplies -->|Aprova| CheckSupplies{Valor total ≤ R$ 100?}
    Supplies -->|Solicita Revisão| Review[Pedido retorna ao elaborador]
    Supplies -->|Cancela| Cancelled([Pedido Cancelado])

    CheckSupplies -->|Sim| Approved([Pedido Aprovado])
    CheckSupplies -->|Não| Manager{Gestor analisa pedido}

    Manager -->|Aprova| CheckManager{Valor total ≤ R$ 1.000?}
    Manager -->|Solicita Revisão| Review
    Manager -->|Cancela| Cancelled

    CheckManager -->|Sim| Approved
    CheckManager -->|Não| Director{Diretor analisa pedido}

    Director -->|Aprova| Approved
    Director -->|Solicita Revisão| Review
    Director -->|Cancela| Cancelled

    Review --> Edit[Elaborador revisa e ajusta o pedido]
    Edit --> Resubmit[Elaborador reenvia pedido]
    Resubmit --> RecalcTotal[Sistema recalcula valor total]
    RecalcTotal --> RedefineChain[Sistema redefine cadeia de aprovação]
    RedefineChain --> Supplies

    style Start fill:#4CAF50,color:#fff
    style Approved fill:#2196F3,color:#fff
    style Cancelled fill:#f44336,color:#fff
    style Review fill:#FF9800,color:#fff
```

## Descrição do Fluxo

1. **Criação**: O elaborador cria o pedido com pelo menos 1 item (RN1)
2. **Cálculo**: O sistema calcula o valor total (quantidade × preço unitário) (RN2)
3. **Envio**: O elaborador envia o pedido para aprovação
4. **Definição da Alçada**: Com base no valor total, define-se a cadeia de aprovação (RN3):
   - Até R$ 100: apenas Suprimentos
   - De R$ 101 a R$ 1.000: Suprimentos + Gestor
   - Acima de R$ 1.000: Suprimentos + Gestor + Diretor
5. **Aprovação Sequencial**: Cada nível aprova, solicita revisão ou cancela (RN4, RN8)
6. **Revisão**: Se solicitada, o pedido retorna ao elaborador e, após reenvio, reinicia toda a cadeia (RN5)
7. **Aprovação Final**: O pedido é aprovado quando todas as aprovações exigidas são obtidas (RN7)
8. **Histórico**: Todas as ações são registradas com data, hora, usuário e tipo (RN6)
