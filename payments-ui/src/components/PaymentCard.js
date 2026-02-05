import React, { useState } from "react";
import "./PaymentForm.css";

export default function PaymentCard({ cardNumber, cardName, expiryDate, cvv }) {
  const [isFlipped, setIsFlipped] = useState(false);

  const formatCardNumber = (number) => {
    const cleaned = (number || "").replace(/\s/g, "");
    const formatted = cleaned.match(/.{1,4}/g)?.join(" ") || "";
    return formatted.padEnd(19, "•");
  };

  return (
    <div className="card-preview-wrapper">
      <div className={`card-flip ${isFlipped ? "flipped" : ""}`}>
        {/* FRONT */}
        <div className="card-face card-front" onClick={() => setIsFlipped(false)}>
          <div className="card-glow" />
          <div className="card-content">
            <div className="card-header">
              <div className="card-brand">Stripe</div>
              <div className="card-chip"></div>
            </div>

            <div className="card-number-display">
              {formatCardNumber(cardNumber || "0000000000000000")}
            </div>

            <div className="card-footer">
              <div>
                <div className="label">Card Holder</div>
                <div className="value">{cardName || "YOUR NAME"}</div>
              </div>
              <div>
                <div className="label">Expires</div>
                <div className="value">{expiryDate || "MM/YY"}</div>
              </div>
            </div>
          </div>
        </div>

        {/* BACK */}
        <div className="card-face card-back" onClick={() => setIsFlipped(false)}>
          <div className="magstripe" />
          <div className="cvv-box">
            <span>{cvv || "•••"}</span>
          </div>
          <div className="cvv-label">CVV</div>
        </div>
      </div>

      <button
        type="button"
        className="view-cvv-btn"
        onClick={() => setIsFlipped((p) => !p)}
      >
        {isFlipped ? "← View Front" : "View CVV →"}
      </button>
    </div>
  );
}
