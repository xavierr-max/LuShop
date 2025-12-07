// Função que será chamada pelo Blazor
window.checkout = async (sessionId, publicKey) => {
    // Inicializa o Stripe com a chave pública
    const stripe = Stripe(publicKey);

    // Redireciona para o checkout seguro do Stripe
    const { error } = await stripe.redirectToCheckout({
        sessionId: sessionId
    });

    if (error) {
        console.error("Erro no redirecionamento do Stripe:", error);
        alert("Houve um erro ao redirecionar para o pagamento.");
    }
};