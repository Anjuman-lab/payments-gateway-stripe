import React, { useEffect, useState } from "react";
import { loadStripe } from "@stripe/stripe-js";
import {
    Elements,
    useStripe,
    useElements,
    CardNumberElement,
    CardExpiryElement,
    CardCvcElement,
} from "@stripe/react-stripe-js";
import axios from "axios";
import PaymentCard from "./PaymentCard";
import "./PaymentForm.css";

const stripePromise = loadStripe("pk_test_51SIpcrPPrWuEeyvEyNKtZmC6S5MfxlySMlH5xIIdfg2TXOVB3EBmD89T9JLVP4E2C0EztAd2xOXOwGEYx3AlJgEh00DeQwzUgm");

const elementStyle = {
    style: {
        base: {
            color: "#fff",
            fontFamily: "Poppins, sans-serif",
            fontSize: "16px",
            "::placeholder": { color: "#b7b6c3" },
            iconColor: "#b388ff",
        },
        invalid: { color: "#ff6b6b" },
        complete: { color: "#4de1ff" },
    },
};

const CheckoutInner = () => {
    const stripe = useStripe();
    const elements = useElements();

    // local UI state
    const [clientSecret, setClientSecret] = useState("");
    const [processing, setProcessing] = useState(false);
    const [message, setMessage] = useState("");
    const [formData, setFormData] = useState({
        name: "",
        cardNumber: "",
        expiry: "",
        cvv: "",
        amount: 100, // Example: £10.00
    });

    // Create PaymentIntent when component mounts
    useEffect(() => {
        let cancelled = false; // avoid setting state after unmount

        const createPaymentIntent = async () => {
            try {
                const res = await axios.post(
                    "https://localhost:7137/api/payments",   // ✅ your controller route
                    {
                        amount: formData.amount,               // cents (e.g., 1000 = $10)
                        currency: "usd",
                    }
                );

                // Backend must return { clientSecret: "pi_..._secret_..." }
                if (!cancelled) setClientSecret(res.data.clientSecret);
            } catch (err) {
                console.error("Error creating PaymentIntent:", err);
            }
        };

        // Only re-create intent when the amount changes
        if (formData?.amount > 0) {
            createPaymentIntent();
        }

        return () => {
            cancelled = true;
        };
    }, [formData.amount]);

    // Handle Stripe payment
    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!stripe || !elements || !clientSecret) return;

        setProcessing(true);
        setMessage("");

        const result = await stripe.confirmCardPayment(clientSecret, {
            payment_method: {
                card: elements.getElement(CardNumberElement),
                billing_details: { name: formData.name },
            },
        });

        if (result.error) {
            setMessage(result.error.message || "Payment failed.");
        } else if (result.paymentIntent?.status === "succeeded") {
            setMessage("✅ Payment successful!");
        }

        setProcessing(false);
    };

    return (
        <div className="payment-container">
            <h2 className="title">Secure Payment</h2>
            <p className="subtitle">
                Enter your payment details to complete the transaction
            </p>

            <div className="payment-box">
                {/* LEFT: Payment Form */}
                <form onSubmit={handleSubmit}>
                    <h3>💳 Card Details</h3>

                    <label>Cardholder Name</label>
                    <input
                        type="text"
                        name="name"
                        placeholder="John Doe"
                        value={formData.name}
                        onChange={(e) =>
                            setFormData({ ...formData, name: e.target.value })
                        }
                        required
                    />

                    <label>Card Number</label>
                    <div className="stripe-input">
                        <CardNumberElement options={elementStyle} />
                    </div>

                    <div className="row">
                        <div>
                            <label>Expiry Date</label>
                            <div className="stripe-input small">
                                <CardExpiryElement options={elementStyle} />
                            </div>
                        </div>

                        <div>
                            <label>CVV</label>
                            <div className="stripe-input small">
                                <CardCvcElement options={elementStyle} />
                            </div>
                        </div>
                    </div>

                    <button type="submit" disabled={!stripe || processing}>
                        {processing ? "Processing..." : "Pay Now"}
                    </button>

                    {message && <p className="message">{message}</p>}
                </form>

                {/* RIGHT: Card Preview + Encryption Info */}
                <div className="card-side">
                    <PaymentCard
                        cardNumber={formData.cardNumber}
                        cardName={formData.name}
                        expiryDate={formData.expiry}
                        cvv={formData.cvv}
                    />

                    <div className="encryption-block">
                        <div className="enc-icon">🔒</div>
                        <div className="enc-text">
                            <div className="enc-title">256-bit Encryption</div>
                            <div className="enc-sub">
                                Your payment information is protected with bank-level security.
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

// Stripe <Elements> wrapper
export default function PaymentForm() {
    return (
        <Elements stripe={stripePromise}>
            <CheckoutInner />
        </Elements>
    );
}
